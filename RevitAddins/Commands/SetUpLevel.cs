using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.IO;

namespace RevitAddins
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class SetUpLevel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<List<string>> csvData = new List<List<string>>();
            string path = "";
            string readResult = CSVData.ReadCSV(ref csvData,ref path);
            if (!String.Equals(readResult, "Succeed"))
            {
                message = readResult;
                return Result.Failed;
            }

            //set up level
            List<string> levelNames = csvData[0];
            List<double> levelElevations = csvData[1].Select(x => double.Parse(x)).ToList();
            List<ElementId> levelIds = new List<ElementId>();
            try
            {
                using(Transaction trans = new Transaction(doc,"Set Up Levels"))
                {
                    trans.Start();

                    for(int i = 0; i < levelNames.Count; i++)
                    {
                        double elevation = UnitUtils.ConvertToInternalUnits(levelElevations[i], DisplayUnitType.DUT_MILLIMETERS);
                        Level level = Level.Create(doc, elevation);
                        level.Name = levelNames[i];
                        levelIds.Add(level.Id);
                    }
                    trans.Commit();
                }
            }
            catch(Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

            //save elementId to csv
            if(csvData.Count <= 2)
            {
                csvData.Add(levelIds.Select(x => x.ToString()).ToList());

            }
            else
            {
                csvData[2] = levelIds.Select(x => x.ToString()).ToList();
            }
            string saveResult = CSVData.SaveCSV(path,csvData);

            return Result.Succeeded;
        }
    }
}
