﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace UpdateForm
{
    public partial class mainForm : Form
    {

        string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\DisplayScreens\";
        string dailyWorkload = "";
        string warehouseUpdate = "";

        string[] lines;
        string[] workloadLines;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                const int resizeArea = 10;
                Point cursorPosition = PointToClient(new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16));
                if (cursorPosition.X >= ClientSize.Width - resizeArea && cursorPosition.Y >= ClientSize.Height - resizeArea)
                {
                    m.Result = (IntPtr)17;
                    return;
                }
            }

            base.WndProc(ref m);
        }




        // Checks if program can access server, and if not, runs locally instead.
        public mainForm()
        {
            InitializeComponent();

            dailyWorkload = folder + "dailyWorkload.txt";
            warehouseUpdate = folder + "warehouseUpdate.txt";

            lines = File.ReadAllLines(warehouseUpdate);
            workloadLines = File.ReadAllLines(dailyWorkload);

        }


        // Reads both text files into the Text boxes on either tab.
        private void mainForm_Load(object sender, EventArgs e)
        {

            label54.Text = DateTime.Now.ToShortDateString();
            label55.Text = DateTime.Now.ToShortDateString();

            TextBox[] tbx = new TextBox[] { textBox1, textBox1, textBox2, textBox3, textBox4, textBox5,
                                            textBox6, textBox7, textBox8, textBox9, textBox10, textBox11,
                                            textBox12, textBox13, textBox14, textBox15, textBox16, textBox17, };

            TextBox[] tbx2 = new TextBox[] { textBox18, textBox18, textBox19, textBox20, textBox21, textBox22,
                                            textBox23, textBox24, textBox25, textBox26, textBox27, textBox28,
                                            textBox29, textBox30, textBox31, textBox32, textBox33, textBox34,
                                            textBox35, textBox36, textBox37, textBox38, textBox39, textBox40,
                                            textBox41, textBox42, textBox43, textBox44, textBox45, textBox46,
                                            textBox47, textBox48, textBox49, textBox50, dpsTotalBox, cpsTotalBox, 
                                            mpsTotalBox, repackTotalBox, hoursBox};

            for (int i = 1; i < tbx.Length; i++) {

                tbx[i].Text = lines[i];

            }

            for (int i = 1; i < tbx2.Length; i++)
            {

                tbx2[i].Text = workloadLines[i];

            }

        }


        // Updates the warehouseUpdate.txt File
        private void updateButton_Click(object sender, EventArgs e)
        {

            TextBox[] tbx = new TextBox[] { textBox1, textBox1, textBox2, textBox3, textBox4, textBox5,
                                            textBox6, textBox7, textBox8, textBox9, textBox10, textBox11,
                                            textBox12, textBox13, textBox14, textBox15, textBox16, textBox17, };

            for (int i = 1; i < tbx.Length; i++)
            {

                lines[i] = tbx[i].Text.Replace(Environment.NewLine, @" ");

            }

            lines[0] = label54.Text;

            File.WriteAllLines(warehouseUpdate, lines);

            MessageBox.Show("File Updated");

        }


        // Updates the dailyWorkload.txt File

        private void updateButton2_Click(object sender, EventArgs e)
        {
            TextBox[] tbx2 = new TextBox[] { textBox18, textBox18, textBox19, textBox20, textBox21, textBox22,
                                            textBox23, textBox24, textBox25, textBox26, textBox27, textBox28,
                                            textBox29, textBox30, textBox31, textBox32, textBox33, textBox34,
                                            textBox35, textBox36, textBox37, textBox38, textBox39, textBox40, 
                                            textBox41, textBox42, textBox43, textBox44, textBox45, textBox46,
                                            textBox47, textBox48, textBox49, textBox50, dpsTotalBox, cpsTotalBox,
                                            mpsTotalBox, repackTotalBox, hoursBox};

            for (int i = 1; i < tbx2.Length; i++)
            {

                workloadLines[i] = tbx2[i].Text.Replace(Environment.NewLine, @" ");

            }

            workloadLines[0] = lines[0] = label55.Text;

            File.WriteAllLines(dailyWorkload, workloadLines);

            MessageBox.Show("File Updated");

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
                this.WindowState = FormWindowState.Maximized;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void titlePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


    }
}