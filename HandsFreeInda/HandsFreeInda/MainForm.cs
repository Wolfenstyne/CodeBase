﻿// INDA Hands Free Clock-In System
// Written by Christopher Olson
// For CVS Health, July 7th 2020

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Automation;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace HandsFreeInda
{

    public partial class MainForm : Form
    {
        Instructions instructionForm = new Instructions();
        AutomationMethods automation = new AutomationMethods();

        // Named elements for Automation use in other methods
        AutomationElement loginFormElement,
        usernameElement,
        passwordElement,
        okButton,
        cancelButton,
        workScreenFormElement,
        workScreenInForDay,
        workScreenStartButton,
        workScreenEndButton,
        lunchButtonElement,
        breakButtonElement,
        otherButtonElement,
        jobCodeElement,
        workScreenOutForDay;

        int idleCount = 0;
        int delayCount = 0;

        string indaProgram = "";
        string username = "";
        string password = "";
        string jobCode = "";
        string parsedPassword = "";
        string check = "";
        string computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
        string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        string clientPath = "X:\\ISShare\\Applications\\INDAHandsFree\\ErrorLogs\\";
        string serverPath = "\\\\rfl6dpspw2c\\CVS-CLIENT-RT\\ISShare\\Applications\\INDAHandsFree\\ErrorLogs\\";

        System.Windows.Forms.Timer textTimer;
        System.Windows.Forms.Timer clockTimer = null;
        System.Windows.Forms.Timer idleTimer;

        // Allows moving the application around by holding mouse button.
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST) m.Result = (IntPtr)(HT_CAPTION);
        }

        // Prepared mouse event to move to the Userbox and click. Resets Cursor position and idle timers.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //Sets an idle timer. After a specified amount of time it triggers an idle action, and every X amount of time therafter until the userBox is altered again.
        private void idleTimer_Tick(Object sender, EventArgs e)
        {
            idleCount++;

            if (idleCount > 20)
            {
                delayCount++;

                // Refocuses the userBox, closes existing INDA processes, and sends a click event to keep Remote session open.
                if (delayCount == 60 || delayCount == 1)
                {

                    if (workScreenFormElement == null && loginFormElement == null)
                    {
                        killINDA();
                        userBox.Focus();
                        this.BringToFront();
                        userBox.Select();
                        SendKeys.Send("{ENTER}");
                        System.Windows.Forms.Cursor.Position = PointToScreen(new Point(userBox.Location.X, userBox.Location.Y));
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0); //make left button down
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); //make left button up
                        if (delayCount != 1)
                        {
                            delayCount = 0;
                        }
                    }

                }
            }

        }

        public MainForm()
        {
            indaProgram = folder + "\\INDA.exe";
            InitializeComponent();

            workScreenFormElement = null;
            loginFormElement = null;

            textTimer = new System.Windows.Forms.Timer();
            textTimer.Interval = 800;
            textTimer.Tick += new EventHandler(textTimer_Tick);

            dateLabel.Text = DateTime.UtcNow.ToString("MM-dd-yyyy");
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += new EventHandler(clockTimer_Tick);
            clockTimer.Enabled = true;

            idleTimer = new System.Windows.Forms.Timer();
            idleTimer.Interval = 1000;
            idleTimer.Tick += new EventHandler(idleTimer_Tick);
            idleTimer.Enabled = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Hide INDA Manual Button
            hideButton();

            //instructionForm.StartPosition = FormStartPosition.Manual;
            //instructionForm.Show();

            userBox.Focus();
            this.BringToFront();

        }

        // Splits the userBox string into 4 parts: User , Password, Job Code, and Check Character for Function Switch
        private void splitBarcode()
        {

            if (userBox.Text.Contains(","))
            {
                string[] str_split = userBox.Text.Split(",".ToCharArray());
                username = str_split[0];
                password = str_split[1];
                jobCode = str_split[2];

                clearFields();

                // If's account for users with 1 or 2 leading 0's in Username or in Password
                if (username.Length == 6)
                {
                    username = "0" + username;
                }

                if (username == "22679" || username == "57606" || username == "22377")
                {
                    username = "00" + username;
                }

                if (password.Length < 4)
                {

                    if (password.Length < 3)
                    {

                        password = "00" + password;

                    }

                    else
                    {
                        password = "0" + password;
                    }

                }

                parsedPassword = Regex.Replace(password, "[+^%~()]", "{$0}");

                check = str_split[3];

                // Empties box and starts INDA
                try
                {
                    INDAstart();
                }
                catch (Exception ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                    indaExitFocus();
                }

            }

        }

        // Opens INDA program, and uses Check variable to determine which clock function is performed
        private void INDAstart()
        {

            try
            {
                runINDA();
            }
            catch (Exception ex)
            {
                errorLog(ex, username);
                generalErrorMessage();
                indaExitFocus();
                return;
            }

            Thread.Sleep(1500);

            try
            {
                indaLogin();
            }

            catch (Exception ex)
            {
                errorLog(ex, username);
                generalErrorMessage();
                indaExitFocus();
                return;
            }
        }

        // Variables sent in to log into INDA
        private void indaLogin()
        {

            idleCount = 0;

            bool flagCheck = false;

            try
            {
                // Set several Form elements of the INDA Username and Password Form
                loginFormElement = automation.setParentElement("Welcome To Indirect Activity (INDA)");
                usernameElement = automation.setChildElementId(loginFormElement, "3", true);
                okButton = automation.setChildElementId(loginFormElement, "7", true);
                cancelButton = automation.setChildElementId(loginFormElement, "6", true);
                automation.setValue(usernameElement, username);
                automation.invokeButtonPress(okButton);

            }

            catch (Exception ex)
            {

                errorLog(ex, username);
                generalErrorMessage();
                return;
            }

            Thread.Sleep(1000);

            if (loginFormElement == null)
            {
                generalErrorMessage();
                indaExitFocus();
                return;
            }

            // Loops looking to see if the Password textbox is enabled. If never enabled assumes a Username error has happened and closes the loop.
            // Example, if user enters a username not found in INDA that will prevent Password box from showing up. 
            try
            {

                int passwordCheck = 0;

                do
                {

                    Thread.Sleep(500);
                    passwordElement = automation.setChildElementId(loginFormElement, "1", false);
                    flagCheck = automation.enabledCheck(passwordElement);
                    passwordCheck++;
                } while (flagCheck == false && passwordCheck < 2); // Waits up to 2 seconds to see the Password Element. 

            }

            catch (Exception ex)
            {
                errorLog(ex, username);
                generalErrorMessage();
                return;
            }

            // If the flagCheck is true, that means the Password box is enabled. Password entry proceeds and function is called depending on check character.
            if (flagCheck == true)
            {

                try
                {

                    automation.setValue(passwordElement, password);
                    automation.invokeButtonPress(okButton);

                }

                catch (Exception ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                    indaExitFocus();
                    return;
                }

                Thread.Sleep(500);

                if (check == "~")
                {
                    indaClockin();
                }
                if (check == "*")
                {
                    indaClockout();
                }
                if (check == "$")
                {
                    indaLunchIn();
                }
                if (check == "@")
                {
                    indaLunchOut();
                }
                if (check == "%")
                {
                    indaBreakIn();
                }
                if (check == "&")
                {
                    indaBreakOut();
                }

            }

            else
            {
                generalErrorMessage();
                indaExitFocus();
                return;
            }

        }

        // INDA Clock In
        private void indaClockin()
        {

            try
            {

                // Sets the 3 main elements needed to clock in. 
                workScreenFormElement = automation.setParentElementWithTime("Indirect Activity Selection | ");
                workScreenInForDay = automation.setChildElementName(workScreenFormElement, "IN-FOR-DAY", false);
                workScreenStartButton = automation.setChildElementId(workScreenFormElement, "17", true);

                bool flagCheck = false;
                flagCheck = automation.enabledCheck(workScreenInForDay);
                if (workScreenInForDay == null || workScreenFormElement == null)
                {
                    generalErrorMessage();

                }

                else if (flagCheck == false)
                {
                    clockInErrorMessage();
                    indaExitFocus();
                }

                else
                {
                    try
                    {
                        Thread.Sleep(500);
                        automation.selectRadioButton(workScreenInForDay);
                        automation.invokeButtonPress(workScreenStartButton);

                        if (jobCode == "D03ST" || jobCode == "D06SE" || jobCode == "D11ST" || jobCode == "D11SE")
                        {

                            indaExitFocus();
                        }

                        else if (username == "1444437" || username == "2003" || username == "20031")
                        {
                            if (jobCode != "")
                            {
                                jobCodeEntry();
                            }
                            indaExitFocus();
                        }

                        else
                        {
                            indaExitFocus();
                        }

                    }
                    catch (ElementNotEnabledException ex)
                    {
                        errorLog(ex, username);
                        clockInErrorMessage();
                        indaExitFocus();
                    }
                    catch (Exception ex)
                    {
                        errorLog(ex, username);
                        generalErrorMessage();
                        indaExitFocus();
                    }

                }

            }

            catch (Exception ex)
            {
                errorLog(ex, username);
                generalErrorMessage();
                indaExitFocus();
            }

        }

        private void jobCodeEntry()
        {

            otherButtonElement = automation.setChildElementId(workScreenFormElement, "14", false);
            automation.selectRadioButton(otherButtonElement);
            jobCodeElement = automation.setChildElementId(workScreenFormElement, "13", false);

            bool flagCheck = false;
            flagCheck = automation.enabledCheck(jobCodeElement);
            if (flagCheck == true && jobCodeElement != null)
            {
                automation.setValue(jobCodeElement, jobCode);
                automation.invokeButtonPress(workScreenStartButton);
                Thread.Sleep(1000);
                indaExitFocus();
            }
            else
            {
                jobCodeErrorMessage();
            }

        }

        // INDA Clock Out
        private void indaClockout()
        {

            try
            {
                workScreenFormElement = automation.setParentElementWithTime("Indirect Activity Selection | ");
                workScreenOutForDay = automation.setChildElementId(workScreenFormElement, "11", false);
                workScreenStartButton = automation.setChildElementId(workScreenFormElement, "17", true);

                bool flagCheck = false;
                flagCheck = automation.enabledCheck(workScreenOutForDay);

                if (flagCheck == false || workScreenOutForDay == null)
                {
                    clockOutErrorMessage();
                }
                else
                {

                    try
                    {
                        Thread.Sleep(500);
                        automation.selectRadioButton(workScreenOutForDay);
                        Thread.Sleep(500);
                        automation.invokeButtonPress(workScreenStartButton);
                    }

                    catch (ElementNotEnabledException ex)
                    {
                        errorLog(ex, username);
                        clockOutErrorMessage();
                    }

                    catch (Exception ex)
                    {
                        errorLog(ex, username);
                        generalErrorMessage();
                    }

                }

            }
            catch (Exception ex)
            {
                errorLog(ex, username);
                generalErrorMessage();
            }

            indaExitFocus();
        }

        // After the loop finishes this makes sure the userBox field is reselected for subsequent users
        private void indaExitFocus()
        {

            username = "";
            password = "";
            jobCode = "";
            parsedPassword = "";
            check = "";
            Thread.Sleep(1000);

            workScreenFormElement = null;
            loginFormElement = null;

            try
            {
                foreach (Process proc in Process.GetProcessesByName("INDA"))
                {
                    proc.Kill();
                }

            }
            catch (Exception ex)
            {
                errorLog(ex, username);
                generalErrorMessage();
            }

            userBox.Focus();
            this.BringToFront();

            Thread.Sleep(1000);
            userBox.Focus();
            this.BringToFront();

        }

        // Empties the userBox text box for next user.
        private void clearFields()
        {
            userBox.Text = "";
            manualUsername.Text = "";
            manualPassword.Text = "";
        }

        // Timer tracks how long since last character enters Input field, to make sure entire string is entered before launching Login function.
        private void textTimer_Tick(Object sender, EventArgs e)
        {
            if (userBox.Focused)
            {
                textTimer.Stop(); //No disposing required, just stop the timer.
                idleCount = 0;
                splitBarcode();

            }
        }

        void clockTimer_Tick(object sender, EventArgs e)
        {

            clockLabel.Text = DateTime.Now.ToString("h:mm:ss tt");
        }

        private void userBox_TextChanged(object sender, KeyEventArgs e)
        {
            textTimer.Start();
        }

        public void runINDA()
        {

            if (File.Exists("R:\\atl\\runtime\\bin\\INDA.exe"))
            {
                Process.Start("R:\\atl\\runtime\\bin\\INDA.exe");
            }
            else if (File.Exists("\\\\rfl6dpspw2c\\CVS-CLIENT-RT\\ISShare\\Applications\\INDAHandsFree\\INDA.exe"))
            {
                Process.Start("\\\\rfl6dpspw2c\\CVS-CLIENT-RT\\ISShare\\Applications\\INDAHandsFree\\INDA.exe");
            }
            else
            {
                Process.Start(indaProgram);
            }

        }

        public void killINDA()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("INDA.exe"))
                {
                    proc.Kill();
                }

                foreach (Process proc in Process.GetProcessesByName("INDA"))
                {
                    proc.Kill();
                }

            }

            catch (Exception ex)
            {
                errorLog(ex, username);
                MessageBox.Show(ex.Message);
            }

            workScreenFormElement = null;
            loginFormElement = null;
            userBox.Focus();
            this.BringToFront();
        }

        public bool ControlIsVisible
        {
            get
            {
                return manualINDA.Visible;
            }
            set
            {
                manualINDA.Visible = value;
            }
        }

        public void hideButton()
        {
            manualINDA.Visible = false;
        }

        public void showButton()
        {
            manualINDA.Visible = true;
        }

        // Button and click events
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            killINDA();
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            killINDA();
            this.Close();
        }

        private void manualINDA_Click(object sender, EventArgs e)
        {
            delayCount = 0;
            idleCount = 0;
            runINDA();
        }

        private void manualPunchButton_Click(object sender, EventArgs e)
        {
            username = manualUsername.Text;
            password = manualPassword.Text;
            parsedPassword = Regex.Replace(password, "[+^%~()]", "{$0}");
            check = "~";
            clearFields();

            if (username != "" && password != "")
            {
                INDAstart();
            }

            else
            {
                MessageBox.Show("Please Enter a Username and Password.");
            }

        }

        private void manualPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                username = manualUsername.Text;
                password = manualPassword.Text;
                parsedPassword = Regex.Replace(password, "[+^%~()]", "{$0}");
                check = "~";
                clearFields();

                if (username != "" && password != "")
                {
                    INDAstart();
                }

                else
                {
                    MessageBox.Show("Please Enter a Username and Password.");
                }
            }
        }

        private void unlockButton_Click(object sender, EventArgs e)
        {
            if (unlockBox.Text == "COVID19")
            {
                if (manualINDA.Visible == true)
                {
                    hideButton();
                }
                else
                {
                    showButton();
                }
            }
            else
            {
                MessageBox.Show("Incorrect Password");
            }
        }

        private void unlockBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (unlockBox.Text == "COVID19")
                {
                    if (manualINDA.Visible == true)
                    {
                        hideButton();
                    }
                    else
                    {
                        showButton();
                    }
                }

                else
                {
                    MessageBox.Show("Incorrect Password, Please Try Again");
                }
            }
        }

        private void clockOutErrorMessage()
        {
            MessageBox.Show("ALREADY CLOCKED OUT. PLEASE CLOCK IN THEN BACK OUT.", "ALREADY CLOCKED OUT ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

        private void clockInErrorMessage()
        {
            MessageBox.Show("ALREADY CLOCKED IN. PLEASE CLOCK OUT THEN BACK IN.", "ALREADY CLOCKED IN ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

        private void generalErrorMessage()
        {
            MessageBox.Show("CLOCK ERROR, PLEASE TRY AGAIN.", "CLOCK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

        private void jobCodeErrorMessage()
        {
            MessageBox.Show("JOB CODE ERROR, PLEASE TRY AGAIN.", "CLOCK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

        // Writes error log to both local and server location.
        private void errorLog(Exception ex, string user)
        {

            string serverFile = "";

            string today = DateTime.Today.ToString("MM'_'dd'_'yyyy");
            string todayLog = "ErrorLog" + today + ".txt";
            string localLogFile = "";

            // Checks to see if either the X:\ or Server Paths are available to find log file folders

            if (Directory.Exists(clientPath))
            {
                serverFile = clientPath + todayLog;
                localLogFile = folder + "\\" + todayLog;
            }
            if (Directory.Exists(serverPath))
            {
                serverFile = serverPath + todayLog;
            }

            if (Directory.Exists(clientPath))
            {
                using (StreamWriter writer = new StreamWriter(localLogFile, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine("User : " + user);
                    writer.WriteLine("Hostname : " + computerName);
                    writer.WriteLine();

                    if (ex != null)
                    {
                        writer.WriteLine(ex.GetType().FullName);
                        writer.WriteLine("Message : " + ex.Message);
                        writer.WriteLine("StackTrace : " + ex.StackTrace);

                        //ex = ex.InnerException;
                    }
                }
            }

            if (Directory.Exists(clientPath) || Directory.Exists(serverPath))
            {
                using (StreamWriter writer = new StreamWriter(serverFile, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine("User : " + user);
                    writer.WriteLine("Hostname : " + computerName);
                    writer.WriteLine();

                    while (ex != null)
                    {
                        writer.WriteLine(ex.GetType().FullName);
                        writer.WriteLine("Message : " + ex.Message);
                        writer.WriteLine("StackTrace : " + ex.StackTrace);

                        ex = ex.InnerException;
                    }

                }
            }

        }

        // Opens logs with button click. Writes a new file if there is none for the day.
        private void logButton_Click_1(object sender, EventArgs e)
        {
            string today = DateTime.Today.ToString("MM'-'dd'-'yyyy");
            string todayLog = "ErrorLog" + today + ".txt";
            string localLogFile = "";
            string serverFile = "";

            // Checks to see if either the X:\ or Server Paths are available to find log file folders
            if (Directory.Exists(clientPath))
            {
                localLogFile = folder + "\\" + todayLog;
                serverFile = clientPath + todayLog;
            }

            if (Directory.Exists(serverPath))
            {
                serverFile = serverPath + todayLog;
            }

            if (Directory.Exists(clientPath))
            {

                if (File.Exists(localLogFile))
                {
                    Process.Start(localLogFile);
                }
                else
                {

                    using (StreamWriter writer = new StreamWriter(localLogFile, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine("Date : " + DateTime.Now.ToString());
                        writer.WriteLine("User : " + username);
                        writer.WriteLine("Hostname : " + computerName);
                        writer.WriteLine();
                    }

                    Process.Start(localLogFile);

                }
            }

            if (Directory.Exists(serverPath))
            {

                if (File.Exists(serverFile))
                {
                    Process.Start(serverFile);
                }
                else
                {

                    using (StreamWriter writer = new StreamWriter(serverFile, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine("Date : " + DateTime.Now.ToString());
                        writer.WriteLine("User : " + username);
                        writer.WriteLine("Hostname : " + computerName);
                        writer.WriteLine();
                    }

                    Process.Start(serverFile);

                }
            }

        }

        // Currently unused methods
        // INDA Lunch In
        private void indaLunchIn()
        {
            workScreenFormElement = automation.setParentElementWithTime("Indirect Activity Selection | ");
            lunchButtonElement = automation.setChildElementId(workScreenFormElement, "18", false);
            workScreenStartButton = automation.setChildElementId(workScreenFormElement, "17", true);

            bool flagCheck = false;
            flagCheck = automation.enabledCheck(lunchButtonElement);

            if (flagCheck == false || lunchButtonElement == null)
            {
                generalErrorMessage();
            }

            else
            {

                try
                {
                    Thread.Sleep(500);
                    automation.selectRadioButton(lunchButtonElement);
                    Thread.Sleep(500);
                    automation.invokeButtonPress(workScreenStartButton);
                }

                catch (ElementNotEnabledException ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                }

                catch (Exception ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                }

            }

            indaExitFocus();
        }

        // INDA Lunch Out
        private void indaLunchOut()
        {
            workScreenFormElement = automation.setParentElementWithTime("Indirect Activity Selection | ");
            lunchButtonElement = automation.setChildElementId(workScreenFormElement, "18", false);
            workScreenEndButton = automation.setChildElementId(workScreenFormElement, "16", true);

            bool flagCheck = false;
            flagCheck = automation.enabledCheck(lunchButtonElement);

            if (flagCheck == false || lunchButtonElement == null)
            {
                generalErrorMessage();
            }

            else
            {

                try
                {
                    Thread.Sleep(500);
                    automation.selectRadioButton(lunchButtonElement);
                    Thread.Sleep(500);
                    automation.invokeButtonPress(workScreenEndButton);
                }

                catch (ElementNotEnabledException ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                }

                catch (Exception ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                }

            }

            indaExitFocus();

        }

        // INDA Break In
        private void indaBreakIn()
        {
            workScreenFormElement = automation.setParentElementWithTime("Indirect Activity Selection | ");
            breakButtonElement = automation.setChildElementId(workScreenFormElement, "15", false);
            workScreenStartButton = automation.setChildElementId(workScreenFormElement, "17", true);

            bool flagCheck = false;
            flagCheck = automation.enabledCheck(breakButtonElement);

            if (flagCheck == false || breakButtonElement == null)
            {
                generalErrorMessage();
            }

            else
            {

                try
                {
                    Thread.Sleep(500);
                    automation.selectRadioButton(breakButtonElement);
                    Thread.Sleep(500);
                    automation.invokeButtonPress(workScreenStartButton);
                }

                catch (ElementNotEnabledException ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                }

                catch (Exception ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                }

            }

            indaExitFocus();
        }

        // INDA Break Out
        private void indaBreakOut()
        {
            workScreenFormElement = automation.setParentElementWithTime("Indirect Activity Selection | ");
            breakButtonElement = automation.setChildElementId(workScreenFormElement, "15", false);
            workScreenEndButton = automation.setChildElementId(workScreenFormElement, "16", true);

            bool flagCheck = false;
            flagCheck = automation.enabledCheck(workScreenEndButton);

            if (flagCheck == false || workScreenEndButton == null)
            {
                generalErrorMessage();
            }

            else
            {

                try
                {
                    Thread.Sleep(500);
                    automation.selectRadioButton(breakButtonElement);
                    Thread.Sleep(500);
                    automation.invokeButtonPress(workScreenEndButton);
                }

                catch (ElementNotEnabledException ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                }

                catch (Exception ex)
                {
                    errorLog(ex, username);
                    generalErrorMessage();
                }

            }

            indaExitFocus();
        }

    }
}