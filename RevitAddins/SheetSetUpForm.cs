using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitAddins
{
    public partial class SheetSetUpForm : Form
    {
        int selectedIndex;
        string startSheetNumberPrefix;
        public int GetSelectedIndex()
        {
            return selectedIndex;
        }
        public string GetStartSheetNumberPrefix()
        {
            return startSheetNumberPrefix;
        }

        public SheetSetUpForm(List<string> titleBlockNames)
        {
            InitializeComponent();
            foreach(string name in titleBlockNames)
            {
                comboBox1.Items.Add(name);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.DialogResult = DialogResult.OK;
            selectedIndex = comboBox1.SelectedIndex;
            startSheetNumberPrefix = textBox1.Text;
            Close();
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
