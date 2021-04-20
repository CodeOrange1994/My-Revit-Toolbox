using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace RevitAddins
{
    public partial class GridStartNumberForm : Form
    {
        public string StartNumber1;
        public string StartNumber2;

        Regex textBox1Format;
        Regex textBox2Format;

        public GridStartNumberForm()
        {
            InitializeComponent();
            textBox1Format = new Regex(@"^([A-Z]-[A-Z]|([A-Z])\2|([A-Z]))$");
            textBox2Format = new Regex(@"^([A-Z]-\d{1,2}|\d{1,2}|\d{1,2}-\d{1,2})$");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!textBox1Format.IsMatch(textBox1.Text))
            {
                label3.ForeColor = Color.Red;
                button1.Enabled = false;
            }
            else
            {
                label3.ForeColor = Color.Black;
                if (textBox2Format.IsMatch(textBox2.Text))
                {
                    button1.Enabled = true;
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!textBox2Format.IsMatch(textBox2.Text))
            {
                label4.ForeColor = Color.Red;
                button1.Enabled = false;
            }
            else
            {
                label4.ForeColor = Color.Black;
                if (textBox1Format.IsMatch(textBox1.Text))
                {
                    button1.Enabled = true;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.DialogResult = DialogResult.OK;
            StartNumber1 = textBox1.Text;
            StartNumber2 = textBox2.Text;
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
