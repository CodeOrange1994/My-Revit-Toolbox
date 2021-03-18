using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using View = Autodesk.Revit.DB.View;

namespace RevitAddins
{
    public partial class ViewChooserForm : System.Windows.Forms.Form
    {
        List<int> selectedViewIndices;
        public List<int> GetSelectedViewIndices()
        {
            return selectedViewIndices;
        }
        public ViewChooserForm(List<string> viewNames)
        {
            InitializeComponent();
            foreach(string name in viewNames)
            {
                checkedListBox1.Items.Add(name);
            }
            button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.DialogResult = DialogResult.OK;
            selectedViewIndices = checkedListBox1.CheckedIndices.Cast<int>().ToList();
            Close();
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count != 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }
    }
}
