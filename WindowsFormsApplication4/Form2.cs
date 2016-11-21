using System;
using System.Threading;
using System.Windows.Forms;
using MainProgram;

namespace WindowsFormsApplication4
{
    public partial class Form2 : Form
    {

        public Form2()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.twitchapps.com/tmi/");
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.Channel;
            textBox2.Text = Properties.Settings.Default.Username;
            textBox3.Text = Properties.Settings.Default.oauth;
            textBox4.Text = Properties.Settings.Default.skipSong;
            checkBox1.Checked = Properties.Settings.Default.enableSkipping;
            textBox5.Text = Properties.Settings.Default.skipValue.ToString();
            textBox6.Text = Properties.Settings.Default.skipTime.ToString();
            textBox7.Text = Properties.Settings.Default.queueLength.ToString();
            checkBox2.Checked = Properties.Settings.Default.keepQueue;
            if (checkBox1.Checked)
            {
                textBox4.Enabled = true;
                textBox5.Enabled = true;
                textBox6.Enabled = true;
            }
            else
            {
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Contains("oauth:")) { }
            else { textBox3.Text = "oauth:" + textBox3.Text; }
            Properties.Settings.Default.Channel = textBox1.Text.ToLower();
            Properties.Settings.Default.Username = textBox2.Text.ToLower();
            Properties.Settings.Default.oauth = textBox3.Text;
            Properties.Settings.Default.skipSong = textBox4.Text;
            Properties.Settings.Default.enableSkipping = checkBox1.Checked;
            Properties.Settings.Default.keepQueue = checkBox2.Checked;
            Properties.Settings.Default.queueLength = Int32.Parse(textBox7.Text);
            if (checkBox1.Checked)
            {
                Properties.Settings.Default.skipValue = Int32.Parse(textBox5.Text);
                Properties.Settings.Default.skipTime = Int32.Parse(textBox6.Text);
            }
            for (int x = 0; x < 6; ++x)
            {
                Properties.Settings.Default.Save();
            }
            MainProgram.Program programz = new Program();
            programz.keepRestart();
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox4.Enabled = true;
                textBox5.Enabled = true;
                textBox6.Enabled = true;
            }
            else
            {
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
            }
        }

        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
