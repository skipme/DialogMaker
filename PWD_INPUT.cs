using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DialogMaker
{
    public partial class PWD_INPUT : Form
    {
        public PWD_INPUT()
        {
            InitializeComponent();
        }

        public string pwd_ref;
        private void button1_Click(object sender, EventArgs e)
        {
            pwd_ref = textBox1.Text.ToString();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
                button1.PerformClick();
        }
    }
}
