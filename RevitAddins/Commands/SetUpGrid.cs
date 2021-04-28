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
    class SetUpGrid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            CSVData csvData = new CSVData();
            string readResult = csvData.ReadCSV();
            if (!String.Equals(readResult, "Succeed"))
            {
                message = readResult;
                return Result.Failed;
            }

            List<double> xGridDist = csvData.Data[0]
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(x => double.Parse(x)).ToList();
            List<double> yGridDist = csvData.Data[1]
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(x => double.Parse(x)).ToList();
            List<double> xGridPos;
            List<double> yGridPos;

            //select a data mode
            ModeSelectionForm modeForm = new ModeSelectionForm("Select Data Mode", "Position Mode", "Distance Mode");
            modeForm.ShowDialog();
            if (modeForm.DialogResult == DialogResult.Cancel)
            {
                message = "Action Canceled";
                return Result.Cancelled;
            }
            else
            {
                if (!modeForm.isMode1)
                {
                    xGridPos = CalculateGridPos(xGridDist);
                    yGridPos = CalculateGridPos(yGridDist);
                }
                else
                {
                    xGridPos = xGridDist.Select(x => UnitUtils.ConvertToInternalUnits(x, DisplayUnitType.DUT_MILLIMETERS)).ToList();
                    yGridPos = yGridDist.Select(x => UnitUtils.ConvertToInternalUnits(x, DisplayUnitType.DUT_MILLIMETERS)).ToList();
                }
            }
            /*
            if (!modeForm.isMode1)
            {
                xGridPos = CalculateGridPos(xGridDist);
                yGridPos = CalculateGridPos(yGridDist);
            }
            else
            {
                xGridPos = xGridDist.Select(x => UnitUtils.ConvertToInternalUnits(x, DisplayUnitType.DUT_MILLIMETERS)).ToList();
                yGridPos = yGridDist.Select(x => UnitUtils.ConvertToInternalUnits(x, DisplayUnitType.DUT_MILLIMETERS)).ToList();
            }*/

            //grid line name
            var curGrids = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Grids)
                .WhereElementIsNotElementType()
                .Cast<Grid>();
            var gridNames = curGrids
                .Select(x => x.Name)
                .ToList();
            var xGridNames = gridNames
                .Where(i => Regex.IsMatch(i, @"^([A-Z]-\d{1,2}|\d{1,2}|\d{1,2}-\d{1,2})$"));
            var yGridNames = gridNames
                .Where(i => Regex.IsMatch(i, @"^([A-Z]-[A-Z]|([A-Z])\2|([A-Z]))$"));
            string lastXGridName = xGridNames.Count() == 0 ? "0" : xGridNames
                .OrderBy(i => i)
                .Last();
            string lastYGridName = yGridNames.Count() == 0 ? null : yGridNames
                .OrderBy(i => i)
                .Last();

            try
            {
                using (Transaction trans = new Transaction(doc, "Set Up Grid"))
                {
                    trans.Start();
                    int xGridIndex = 1;
                    foreach (double pos in xGridPos)
                    {
                        XYZ p1 = new XYZ(pos, -3, 0);
                        XYZ p2 = new XYZ(pos, yGridPos[yGridPos.Count - 1] + 3, 0);
                        Line gridLine = Line.CreateBound(p1, p2);
                        Grid grid = Grid.Create(doc, gridLine);
                        //naming for x direction grid
                        grid.Name = Naming.NextString(lastXGridName, xGridIndex);
                        xGridIndex++;
                    }

                    int yGridIndex = 0;
                    foreach (double pos in yGridPos)
                    {
                        XYZ p1 = new XYZ(-3, pos, 0);
                        XYZ p2 = new XYZ(xGridPos[xGridPos.Count - 1] + 3, pos, 0);
                        Line gridLine = Line.CreateBound(p1, p2);
                        Grid grid = Grid.Create(doc, gridLine);
                        //naming for y direction grid
                        if (lastYGridName == null)
                        {
                            grid.Name = Naming.NextString("A", yGridIndex);
                            yGridIndex++;
                        }
                        else
                        {
                            yGridIndex++;
                            grid.Name = Naming.NextString(lastYGridName, yGridIndex);
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

        private List<double> CalculateGridPos(List<double> gridDist)
        {
            List<double> grid = new List<double>();
            double distToOrigin = 0.0;
            foreach(double d in gridDist)
            {
                distToOrigin += UnitUtils.ConvertToInternalUnits(d,DisplayUnitType.DUT_MILLIMETERS);
                grid.Add(distToOrigin);
            }
            return grid;
        }
    }
}
