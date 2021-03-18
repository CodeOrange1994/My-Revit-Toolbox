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
    class SetUpGrid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string yGridName = "ABCDEFGHIJKLMNOPQRSTUVWXY";

            List<List<string>> csvData = new List<List<string>>();
            string path = "";
            string readResult = CSVData.ReadCSV(ref csvData, ref path);
            if (!String.Equals(readResult, "Succeed"))
            {
                message = readResult;
                return Result.Failed;
            }

            List<double> xGridDist = csvData[0]
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(x => double.Parse(x)).ToList();
            List<double> yGridDist = csvData[1]
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(x => double.Parse(x)).ToList();

            List<double> xGridPos = CalculateGridPos(xGridDist);
            List<double> yGridPos = CalculateGridPos(yGridDist);

            
            try
            {
                using (Transaction trans = new Transaction(doc, "Set Up Grid"))
                {
                    trans.Start();
                    int xGridIndex = 0;
                    int yGridIndex = 0;
                    foreach (double pos in xGridPos)
                    {
                        xGridIndex++; 
                        XYZ p1 = new XYZ(pos, -3, 0);
                        XYZ p2 = new XYZ(pos, yGridPos[yGridPos.Count - 1] + 3, 0);
                        Line gridLine = Line.CreateBound(p1, p2);
                        Grid grid = Grid.Create(doc, gridLine);
                        grid.Name = xGridIndex.ToString();
                    }
                    foreach (double pos in yGridPos)
                    {
                        XYZ p1 = new XYZ(-3, pos, 0);
                        XYZ p2 = new XYZ(xGridPos[xGridPos.Count - 1] + 3, pos, 0);
                        Line gridLine = Line.CreateBound(p1, p2);
                        Grid grid = Grid.Create(doc, gridLine);
                        if (yGridIndex < 25)
                        {
                            grid.Name = yGridName[yGridIndex].ToString();
                        }
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
