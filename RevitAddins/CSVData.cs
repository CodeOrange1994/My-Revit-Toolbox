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
        public List<List<string>> Data { get; set; }
        string Path;

        public CSVData() { }

        public CSVData(List<List<string>> csvData)
        {
            Data = csvData;
        }

        public string ReadCSV()
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
            Path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);

            //parse file
            try
            {
                List<List<string>> rawData = new List<List<string>>();
                using (StreamReader s = new StreamReader(@Path))
                {
                    while (!s.EndOfStream)
                    {
                        string line = s.ReadLine();
                        string[] data_ = line.Split(new char[] { ',' });
                        rawData.Add(data_.ToList());
                    }
                }

                //transpose each column data to seperate list
                Data = rawData.SelectMany(inner => inner.Select((item, index) => new { item, index }))
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

        public string SaveCSV()
        {
            if (Path == null || Path == "")
            {
                //select a location to save file
                FileSaveDialog fileSave = new FileSaveDialog("CSV|*.csv");
                fileSave.Title = "Save CSV";
                fileSave.Show();
                ModelPath modelPath = fileSave.GetSelectedModelPath();
                Path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
            }

            return SaveCSV(Path);
        }

        public string SaveCSV(string path)
        {
            try
            {
                //transpose data to group list as row
                var data = Data.SelectMany(inner => inner.Select((item, index) => new { item, index }))
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
