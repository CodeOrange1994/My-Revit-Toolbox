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
    public partial class ModeSelectionForm : Form
    {
        public bool isMode1;

        public ModeSelectionForm(string formTitle, string radioButton1Name, string radioButton2Name)
        {
            InitializeComponent();
            Name = formTitle;
            radioButton1.Text = radioButton1Name;
            radioButton2.Text = radioButton2Name;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
                button1.Enabled = true;
            }
            else if(!radioButton2.Checked)
            {
                button1.Enabled = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
                button1.Enabled = true;
            }
            else if (!radioButton1.Checked)
            {
                button1.Enabled = false;
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            isMode1 = radioButton1.Checked;
            Close();
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
            return;
        }
    }
}
