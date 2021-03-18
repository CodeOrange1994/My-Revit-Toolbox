using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace RevitAddins
{
    [TransactionAttribute(TransactionMode.Manual)]
    class AddViewToSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Get All the views
            List<View> docViews = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Cast<View>()
                .Where(x=>!x.IsTemplate)//get rid of template views
                .OrderBy(x => x.ViewType)
                .ToList();
            List<string> viewNames = docViews.Select(x => x.Name+" - "+x.ViewType.ToString()).ToList();

            //ask user to choose views
            ViewChooserForm viewForm = new ViewChooserForm(viewNames);
            viewForm.ShowDialog();
            List<int> selectedViewIndices = viewForm.GetSelectedViewIndices();
            if(selectedViewIndices == null)
            {
                message = "None of the views is selected";
                return Result.Cancelled;
            }

            //get selected views' id
            List<ElementId> selectedViewIds = selectedViewIndices.Select(x => docViews[x].Id).ToList();

            //Get all the title blocks
            List<FamilySymbol> titleBlocks = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .ToList();
            List<string> titleBlockNames = titleBlocks.Select(x => x.Name).ToList();

            //ask user to choose a title block
            SheetSetUpForm titleBlockForm = new SheetSetUpForm(titleBlockNames);
            titleBlockForm.ShowDialog();
            int selectedTitleBlockIndex = titleBlockForm.GetSelectedIndex();
            if (selectedTitleBlockIndex == -1)
            {
                message = "None of the title Block is selected";
                return Result.Cancelled;
            }
            string startSheetNumberPrefix = titleBlockForm.GetStartSheetNumberPrefix();
            //TaskDialog.Show("Selected title Block Index", selectedTitleBlockIndex.ToString());


            try
            {
                using (Transaction trans = new Transaction(doc, "Add View to Sheet"))
                {
                    trans.Start();
                    int sheetNumber = 1;
                    
                    foreach(ElementId id in selectedViewIds)
                    {
                        //create sheets for views
                        ViewSheet vSheet = ViewSheet.Create(doc, titleBlocks[selectedTitleBlockIndex].Id);
                        vSheet.Name = doc.GetElement(id).Name;
                        vSheet.SheetNumber = startSheetNumberPrefix + sheetNumber.ToString("D2");
                        sheetNumber++;

                        //add views to sheets
                        BoundingBoxUV outline = vSheet.Outline;
                        double xu = (outline.Max.U + outline.Min.U) / 2;
                        double yu = (outline.Max.V + outline.Min.V) / 2;
                        XYZ centerPoint = new XYZ(xu, yu, 0);
                        Viewport viewport = Viewport.Create(doc, vSheet.Id, id, centerPoint);
                    }
                    trans.Commit();
                }
            }
            catch(Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
            TaskDialog.Show("First Selected View Name", viewNames[selectedViewIndices[0]]);

            return Result.Succeeded;
        }
    }
}
