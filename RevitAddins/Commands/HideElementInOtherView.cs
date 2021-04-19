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
    class HideElementInOtherView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                IList<Reference> pickedObjs = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
                List<ElementId> pickedEleIds = pickedObjs.Select(x => x.ElementId).ToList();


                List<View> views = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Views)
                    .WhereElementIsNotElementType()
                    .Cast<View>()
                    .ToList();

                using (Transaction trans = new Transaction(doc,"Hide Element in Other View"))
                {
                    trans.Start();
                    foreach(View v in views)
                    {
                        v.HideElements(pickedEleIds);
                    }
                    doc.ActiveView.UnhideElements(pickedEleIds);
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
