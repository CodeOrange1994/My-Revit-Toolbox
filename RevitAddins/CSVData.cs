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
    class CSVData
    {
        public static string ReadCSV(ref List<List<string>> csvData, ref string path)
        {
            //select a csv file
            FileOpenDialog fileOpen = new FileOpenDialog("CSV|*.csv");
            fileOpen.Title = "Open CSV";
            fileOpen.Show();
            ModelPath modelPath = fileOpen.GetSelectedModelPath();
            if (modelPath == null)
            {
                return "No CSV File Selected";
            }
            path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);

            //parse file
            try
            {
                List<List<string>> rawData = new List<List<string>>();
                using (StreamReader s = new StreamReader(@path))
                {
                    while (!s.EndOfStream)
                    {
                        string line = s.ReadLine();
                        string[] data = line.Split(new char[] { ',' });
                        rawData.Add(data.ToList());
                    }
                }

                //transpose each column data to seperate list
                csvData = rawData.SelectMany(inner => inner.Select((item, index) => new { item, index }))
                    .GroupBy(i => i.index, i => i.item)
                    .Select(g => g.ToList())
                    .ToList();
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return "Succeed";
        }

        public static string SaveCSV(List<List<string>> csvData)
        {
            //select a location to save file
            FileSaveDialog fileSave = new FileSaveDialog("CSV|*.csv");
            fileSave.Title = "Save CSV";
            fileSave.Show();
            ModelPath modelPath = fileSave.GetSelectedModelPath();
            string path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);

            return SaveCSV(path, csvData);
        }

        public static string SaveCSV(string path, List<List<string>> csvData)
        {
            try
            {
                //transpose data to group list as row
                var data = csvData.SelectMany(inner => inner.Select((item, index) => new { item, index }))
                    .GroupBy(i => i.index, i => i.item)
                    .Select(g => g.ToList())
                    .ToList();

                //write csv
                var csv = new StringBuilder();
                foreach (List<string> rowData in data)
                {
                    var newLine = String.Join(",", rowData.Select(x => x.ToString()));
                    csv.AppendLine(newLine);
                }
                File.WriteAllText(@path, csv.ToString());
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "Succeed";
        }
    }
}
