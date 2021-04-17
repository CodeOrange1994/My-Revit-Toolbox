using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddins
{
    class ExternalApplication : IExternalApplication
    {
        string path = Assembly.GetExecutingAssembly().Location;
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string panelName = "Hugo's Toolbox";
            RibbonPanel panel = application.CreateRibbonPanel(panelName);

            PulldownButtonData data = new PulldownButtonData("Options", "Hugo's Toolbox");
            RibbonItem item = panel.AddItem(data);

            PulldownButton optionButtons = item as PulldownButton;

            List<string> buttonNames = new List<string>();
            buttonNames.Add("SetUpLevel");
            buttonNames.Add("UpdateLevel");
            buttonNames.Add("SetUpGrid");
            buttonNames.Add("HideElementInOtherView");
            buttonNames.Add("AddViewToSheet");
            buttonNames.Add("AlignViewport");

            AddButtons(buttonNames, optionButtons);
            return Result.Succeeded;
        }

        void AddButtons(List<string> buttonNames, PulldownButton optionButtons)
        {
            foreach(string buttonName in buttonNames)
            {
                string commandName = string.Concat(buttonName.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                optionButtons.AddPushButton(new PushButtonData(buttonName, commandName, path, "RevitAddins."+buttonName));
            }
        }
    }
}
