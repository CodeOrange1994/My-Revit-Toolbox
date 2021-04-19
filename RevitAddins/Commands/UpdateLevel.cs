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
    class UpdateLevel:IExternalCommand
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

            try
            {
                using(Transaction trans = new Transaction(doc,"Update Levels"))
                {
                    trans.Start();
                    for (int i = 0; i < csvData.Data[1].Count; i++)
                    {
                        ElementId id = new ElementId(Convert.ToInt32(csvData.Data[2][i]));
                        Level level = doc.GetElement(id) as Level;
                        level.Name = csvData.Data[0][i];
                        level.Elevation = UnitUtils.ConvertToInternalUnits(Convert.ToDouble(csvData.Data[1][i]), DisplayUnitType.DUT_MILLIMETERS);
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
