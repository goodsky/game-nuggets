using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TugOfWar
{
    public partial class SimpleInput : Form
    {
        String title, words;

        public string res;

        public SimpleInput(String title, String words)
        {
            this.title = title;
            this.words = words;

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            res = textBox.Text;

            DialogResult = DialogResult.OK;
        }

        private void SimpleInput_Load(object sender, EventArgs e)
        {
            this.Text = title;
            this.label.Text = words;
        }

    }
}
