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
    public partial class ListItemChooserForm : Form
    {
        int selectedIndex;
        public int GetSelectedIndex()
        {
            return selectedIndex;
        }
        public ListItemChooserForm(List<string> Names, string formTitle)
        {
            InitializeComponent();
            foreach (string name in Names)
            {
                comboBox1.Items.Add(name);
            }
            Name = formTitle;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.DialogResult = DialogResult.OK;
            selectedIndex = comboBox1.SelectedIndex;
            Close();
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.DialogResult = DialogResult.Cancel;
            Close();
            return;
        }
    }
}
