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
    class SetUpGrid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

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
            if (!modeForm.isMode1)
            {
                xGridPos = CalculateGridPos(xGridDist);
                yGridPos = CalculateGridPos(yGridDist);
            }
            else
            {
                xGridPos = xGridDist;
                yGridPos = yGridDist;
            }

            //grid line name
            List<string> gridNames = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Grids)
                .WhereElementIsNotElementType()
                .Cast<Grid>()
                .Select(x => x.Name)
                .ToList();

            try
            {
                using (Transaction trans = new Transaction(doc, "Set Up Grid"))
                {
                    trans.Start();
                    int xGridIndex = 0;
                    int yGridIndex = 0;
                    foreach (double pos in xGridPos)
                    {
                        XYZ p1 = new XYZ(pos, -3, 0);
                        XYZ p2 = new XYZ(pos, yGridPos[yGridPos.Count - 1] + 3, 0);
                        Line gridLine = Line.CreateBound(p1, p2);
                        Grid grid = Grid.Create(doc, gridLine);
                        //naming for x direction grid
                        xGridIndex++;
                        while (gridNames.Contains(xGridIndex.ToString()))
                        {
                            xGridIndex++;
                        }
                        grid.Name = xGridIndex.ToString();
                    }
                    foreach (double pos in yGridPos)
                    {
                        XYZ p1 = new XYZ(-3, pos, 0);
                        XYZ p2 = new XYZ(xGridPos[xGridPos.Count - 1] + 3, pos, 0);
                        Line gridLine = Line.CreateBound(p1, p2);
                        Grid grid = Grid.Create(doc, gridLine);
                        //naming for y direction grid
                        string tempGridName = yGridPos.Count < 26 ? alphabet[yGridIndex].ToString() : String.Format("{0}-{1}", alphabet[yGridIndex/26], alphabet[yGridIndex % 26]);
                        while (gridNames.Contains(tempGridName))
                        {
                            yGridIndex++;
                            tempGridName = yGridPos.Count < 26 ? alphabet[yGridIndex].ToString() : String.Format("{0}-{1}", alphabet[yGridIndex / 26], alphabet[yGridIndex % 26]);
                        }
                        grid.Name = tempGridName;
                        yGridIndex++;
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
