using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RevitAddins.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    class UpdateGridNumber : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //select mode
            ModeSelectionForm modeForm = new ModeSelectionForm("Selection Mode", "Selelct Grids", "All The grids");
            modeForm.ShowDialog();
            if (modeForm.DialogResult == DialogResult.Cancel)
            {
                message = "Action Canceled";
                return Result.Cancelled;
            }
            List<Grid> grids = new List<Grid>();

            if (modeForm.isMode1)
            {
                IList<Reference> pickedObjs = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new SelectionFilters.GridElementFilter(), "Select Grids");
                grids = pickedObjs.Select(x => doc.GetElement(x.ElementId)).Cast<Grid>().ToList();
            }

            else
            {
                grids = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Grids)
                    .WhereElementIsNotElementType()
                    .Cast<Grid>()
                    .ToList();
            }

            GridStartNumberForm startNumberForm = new GridStartNumberForm();
            startNumberForm.ShowDialog();
            if (modeForm.DialogResult == DialogResult.Cancel)
            {
                message = "Action Canceled";
                return Result.Cancelled;
            }
            string startNumber1 = startNumberForm.StartNumber1;
            string startNumber2 = startNumberForm.StartNumber2;
            var gridGroup = grids.GroupBy(i => Regex.IsMatch(i.Name, @"^([A-Z]-\d{1,2}|\d{1,2}|\d{1,2}-\d{1,2})$"));

            try
            {
                using (Transaction trans = new Transaction(doc, "Update gird Numbers"))
                {
                    trans.Start();
                    foreach (IGrouping<bool, Grid> group in gridGroup)
                    {
                        if (group.Key)
                        {
                            List<Grid> gridList = group.ToList().OrderBy(g => g.Curve.GetEndPoint(0).X).ToList();
                            int xGridIndex = 0;
                            foreach (Grid g in gridList)
                            {
                                g.Name = Naming.NextString(startNumber2, xGridIndex);
                                xGridIndex++;
                            }
                        }
                        else
                        {
                            List<Grid> gridList = group.ToList().OrderBy(g => g.Curve.GetEndPoint(0).Y).ToList();
                            int yGridIndex = 0;
                            foreach (Grid g in gridList)
                            {
                                g.Name = Naming.NextString(startNumber1, yGridIndex);
                                yGridIndex++;
                            }
                        }
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
