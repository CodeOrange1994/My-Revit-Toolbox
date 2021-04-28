using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace RevitAddins.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    class AddViewToSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Get viewId of views already appear on a sheet
            List<ElementId> viewInSheetIds = new FilteredElementCollector(doc)
                .OfClass(typeof(Viewport))
                .WhereElementIsNotElementType()
                .Cast<Viewport>()
                .Select(x => x.ViewId)
                .ToList();

            //Get All the views
            List<View> docViews = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Cast<View>()
                .Where(x=>!x.IsTemplate && !(viewInSheetIds.Contains(x.Id)))//get rid of template views and views alread on sheet
                .OrderBy(x => x.ViewType.ToString() + " - " + x.Name)
                .ToList();

            //sort view
            List<string> viewNames = docViews
                .Select(x => x.ViewType.ToString() + " - " + x.Name)
                .ToList();

            //ask user to choose views
            ListMultiChooserForm viewForm = new ListMultiChooserForm(viewNames,"Choose Views");
            viewForm.ShowDialog();
            List<int> selectedViewIndices = viewForm.GetSelectedIndices();
            if(selectedViewIndices == null)
            {
                message = "None of the views is selected";
                return Result.Cancelled;
            }

            //get selected views' id
            List<ElementId> selectedViewIds = selectedViewIndices
                .Select(x => docViews[x].Id)
                .OrderBy(x => doc.GetElement(x).Name)
                .ToList();

            //Get all the title blocks
            List<FamilySymbol> titleBlocks = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .ToList();
            List<string> titleBlockNames = titleBlocks
                .Select(x => String.Format("{0} - {1}",x.FamilyName, x.Name))
                .OrderBy(x => x)
                .ToList();

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

            //TaskDialog.Show("First Selected View Name", viewNames[selectedViewIndices[0]]);
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

            return Result.Succeeded;
        }
    }
}
