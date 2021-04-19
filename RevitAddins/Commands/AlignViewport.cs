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
    class AlignViewport : IExternalCommand
    {
        //WIP
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Get All the views
            List<ViewSheet> docSheets = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Sheets)
                .WhereElementIsNotElementType()
                .Cast<ViewSheet>()
                .OrderBy(x => x.SheetNumber)
                .ToList();
            List<string> sheetNames = docSheets.Select(x => x.SheetNumber.ToString() + " - " + x.Name).ToList();

            //ask user to choose views
            ListMultiChooserForm sheetForm = new ListMultiChooserForm(sheetNames, "Choose Sheets");
            sheetForm.ShowDialog();
            List<int> selectedSheetIndices = sheetForm.GetSelectedIndices();
            if (selectedSheetIndices == null)
            {
                message = "None of the views is selected";
                return Result.Cancelled;
            }

            List<string> selectedSheetNames = selectedSheetIndices.Select(x => sheetNames[x]).ToList();

            //check sheet viewports counts if different raise error


            //ask user to choose a Reference Sheet
            ListItemChooserForm referenceSheetForm = new ListItemChooserForm(selectedSheetNames, "Choose Reference Sheet");
            referenceSheetForm.ShowDialog();
            int referenceSheetIndex = referenceSheetForm.GetSelectedIndex();
            if (referenceSheetIndex == -1)
            {
                message = "None of the sheet is selected";
                return Result.Cancelled;
            }
            ViewSheet referenceVSheet = docSheets[selectedSheetIndices[referenceSheetIndex]];
            TaskDialog.Show("Reference sheet is", selectedSheetNames[referenceSheetIndex]);

            try
            {
                using (Transaction trans = new Transaction(doc, "Align viewports"))
                {
                    trans.Start();
                    //get view crops and position from the view of reference sheet
                    Viewport referenceVP = doc.GetElement(referenceVSheet.GetAllViewports().First()) as Viewport;
                    View referenceView = doc.GetElement(referenceVP.ViewId) as View;
                    //XYZ refXYZ = referenceVP.GetBoxOutline().MinimumPoint;
                    XYZ refXYZ = referenceVP.GetBoxCenter();
                    CurveLoop cropRegion = referenceView.GetCropRegionShapeManager().GetCropShape().First();
                    referenceView.CropBoxActive = true;
                    //apply view crop to all the selected sheets
                    foreach (int sheetIndex in selectedSheetIndices)
                    {
                        ElementId viewId = docSheets[sheetIndex].GetAllPlacedViews().First();
                        View view = doc.GetElement(viewId) as View;
                        //bool cropVisible = view.CropBoxVisible;
                        view.CropBoxActive = true;
                        view.GetCropRegionShapeManager().SetCropShape(cropRegion);


                        ElementId viewportId = docSheets[sheetIndex].GetAllViewports().First();
                        Viewport viewport = doc.GetElement(viewportId) as Viewport;
                        //XYZ vpXYZ = viewport.GetBoxOutline().MinimumPoint;
                        //XYZ vector = refXYZ-vpXYZ;
                        //ElementTransformUtils.MoveElement(doc, viewportId, vector);
                        viewport.SetBoxCenter(refXYZ);
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
