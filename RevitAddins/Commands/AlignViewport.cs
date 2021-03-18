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
    class AlignViewport : IExternalCommand
    {
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
                message = "None of the title Block is selected";
                return Result.Cancelled;
            }
            TaskDialog.Show("reference title Block is", selectedSheetNames[referenceSheetIndex]);

            return Result.Succeeded;
        }
    }
}
