﻿using System;
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
using System.Windows.Input;
using System.Security.Cryptography;
using System.Windows.Automation;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading.Tasks;
using Microsoft.Test.Input;


namespace ReportGenerator
{
    public class Program
    {
        static void Main(string[] args)
        {
            Program programMethods = new Program();
            AutomationMethods automation = new AutomationMethods();
            AutomationMethods automation2 = new AutomationMethods();
            string programPath = System.IO.Directory.GetCurrentDirectory();
            string reportPath = programPath + "\\Reports\\";

            if (!Directory.Exists(reportPath))
            {
                Directory.CreateDirectory(reportPath);
            }
          
            string clientPath = "X:\\abl\\runtime\\bin\\";
            string serverPath = "R:\\abl\\runtime\\bin\\";
            //string absolutePath = "\\\\rfl6dpsapw1v\\WMS-CLIENT\\abl\\runtime\\bin\\";
            string bmisFile = "BMIS.exe";
            string bmisProgram = "BMIS.exe";
            string savedFileOverview = "Overview.csv";
            string overviewReport = "WR002: System Performance Overview"; 
            string savedFileRepack = "Repack.csv";
            string repackReport = "REP-002: Repack Performance - Hourly";
            int time = 0;
           
            string pathSelection = clientPath + bmisFile;
            if (Directory.Exists(clientPath))
            {
                pathSelection = clientPath + bmisFile;
            }
            else if (Directory.Exists(serverPath))
            {
                pathSelection = serverPath + bmisFile;
            }
            else 
            {
                pathSelection = bmisProgram;
            }


            Console.WriteLine("Opening BMIS");

            for (time =5; time > 0; time--){
                Console.WriteLine("Opening in: " + time.ToString() + " seconds.");
                Thread.Sleep(1000);
            }


            Process.Start(pathSelection);
            Thread.Sleep(3000);
            programMethods.loginMethod(automation, programMethods);
            Thread.Sleep(3000);
            programMethods.grabReport(automation, programMethods, overviewReport);
            Thread.Sleep(15000);
            programMethods.exportFile(automation, programMethods, reportPath, savedFileOverview);
            programMethods.closeBMIS(automation2, programMethods, pathSelection, reportPath, repackReport, savedFileRepack);

        }

        public void loginMethod(AutomationMethods automation, Program programMethods)
        {

            AutomationElement
               loginFormElement,
                loginUserElement,
                loginOkButton,
                loginPassElement;


            string username = "20031";
            string password = "1337";
            loginFormElement = automation.setParentElement("Log on BMIS");


            loginUserElement = automation.setChildElementById(loginFormElement, "4", true);
            loginPassElement = automation.setChildElementById(loginFormElement, "5", true);
            loginOkButton = automation.setChildElementById(loginFormElement, "8", true);
            automation.setValue(loginUserElement, username);
            automation.setValue(loginPassElement, password);

            try
            {

                automation.invokeButtonPress(loginOkButton);
            }

            catch (Exception ex)
            {
                programMethods.generalErrorMessage();
            }

        
        }


        private void grabReport(AutomationMethods automation, Program programMethods, string selectedReport)
        {

            AutomationElement
                   mainFormElement,
                   sampleReportElement,
                   statisticsButtonElement;


            mainFormElement = automation.setParentElement("BMIS V3.4.9");

            int loopCount = 0;
            do
            {
                sampleReportElement = automation.setChildElementByName(mainFormElement, selectedReport, true);
                loopCount++;
                Thread.Sleep(1000);
            } while (sampleReportElement == null && loopCount < 5);
          


            statisticsButtonElement = automation.setChildElementByName(mainFormElement, "F2 Statistics", true);
            automation.selectItem(sampleReportElement);
            Thread.Sleep(1000);
            SendKeys.SendWait("{ENTER}");
        }

        private void exportFile(AutomationMethods automation, Program programMethods, string reportPath, string savedFile) 
        {

            AutomationElement
                mainFormElement,
                exportButton,
                exportAsComboBox,
                okButton;

            mainFormElement = automation.setParentElement("BMIS V3.4.9");

            int count = 0;
            do
            {
                exportButton = automation.setChildElementByName(mainFormElement, "Export", true);
                count++;
                Thread.Sleep(1000);

            } while (exportButton == null & count < 10);

            automation.invokeButtonPress(exportButton);

            Thread.Sleep(500);

            count = 0;
            do
            {
                exportAsComboBox = automation.setChildElementByName(mainFormElement, "Export as", true);
                count++;
                Thread.Sleep(1000);

            } while (exportAsComboBox == null & count < 10);

            
            if (exportAsComboBox != null) {
                SendKeys.SendWait("{UP}");
                SendKeys.SendWait("{TAB}");
                SendKeys.SendWait(" ");
                okButton = automation.setChildElementByName(mainFormElement, "F2 Ok", true);
                automation.invokeButtonPress(okButton);
                saveFile(automation, programMethods, reportPath, savedFile);
            }
           
        
        
        }

        private void saveFile(AutomationMethods automation, Program programMethods,  string reportPath, string savedFile)
        {

            Thread.Sleep(1000);
            

            AutomationElement
                    mainFormElement,
                    saveWindow;

            mainFormElement = automation.setParentElement("BMIS V3.4.9");
            int loopcount = 0;
            do
            {
                loopcount++;
                saveWindow = automation.setChildElementByName(mainFormElement, "Save As", true);
                Thread.Sleep(500);
            } while (saveWindow == null && loopcount < 5);

            if (saveWindow != null) {
                SendKeys.SendWait(savedFile);
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{ENTER}");
                SendKeys.SendWait(reportPath);
                Thread.Sleep(1000);
                SendKeys.SendWait("{ENTER}");
                Thread.Sleep(1000);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200)
                    ;
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{ENTER}");
                Thread.Sleep(2000);
                SendKeys.SendWait("{LEFT}");
                Thread.Sleep(200);
                SendKeys.SendWait("{ENTER}");

            }


        }

        private void closeBMIS(AutomationMethods automation2, Program programMethods, string pathSelection, string reportPath, string repackReport, string savedFileRepack)
        {

            Thread.Sleep(4000);

            
            try
            {

                SendKeys.SendWait("%{F4}");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                
            }

            int time = 0;

            for (time = 10; time > 0; time--)
            {
                Console.WriteLine(time.ToString() + " seconds left until opening Repack");
                Thread.Sleep(1000);

   
            }

            Process.Start(pathSelection);
            Thread.Sleep(3000);
            programMethods.loginMethod(automation2, programMethods);
            Thread.Sleep(3000);
            programMethods.grabReport(automation2, programMethods, repackReport);
            Thread.Sleep(15000);
            programMethods.exportFile(automation2, programMethods, reportPath, savedFileRepack);
            programMethods.closeBMIS2();


        }


        public void loginMethod2(AutomationMethods automation, Program programMethods)
        {

            AutomationElement
               loginFormElement2,
                loginUserElement2,
                loginOkButton2,
                loginPassElement2;


            string username = "20031";
            string password = "1337";
            loginFormElement2 = automation.setParentElement("Log on BMIS");


            loginUserElement2 = automation.setChildElementById(loginFormElement2, "4", true);
            loginPassElement2 = automation.setChildElementById(loginFormElement2, "5", true);
            loginOkButton2 = automation.setChildElementById(loginFormElement2, "8", true);
            automation.setValue(loginUserElement2, username);
            automation.setValue(loginPassElement2, password);

            try
            {

                automation.invokeButtonPress(loginOkButton2);
            }

            catch (Exception ex)
            {
                programMethods.generalErrorMessage();
            }


        }


        private void grabReport2(AutomationMethods automation, Program programMethods, string selectedReport)
        {

            AutomationElement
                   mainFormElement2,
                   sampleReportElement2,
                   statisticsButtonElement2;


            mainFormElement2 = automation.setParentElement("BMIS V3.4.9");

            int loopCount = 0;
            do
            {
                sampleReportElement2 = automation.setChildElementByName(mainFormElement2, selectedReport, true);
                loopCount++;
                Thread.Sleep(1000);
            } while (sampleReportElement2 == null && loopCount < 5);



            statisticsButtonElement2 = automation.setChildElementByName(mainFormElement2, "F2 Statistics", true);
            automation.selectItem(sampleReportElement2);
            Thread.Sleep(1000);
            SendKeys.SendWait("{ENTER}");
        }

        private void exportFile2(AutomationMethods automation, Program programMethods, string reportPath, string savedFile)
        {

            AutomationElement
                mainFormElement2,
                exportButton2,
                exportAsComboBox2,
                okButton2;

            mainFormElement2 = automation.setParentElement("BMIS V3.4.9");

            int count = 0;
            do
            {
                exportButton2 = automation.setChildElementByName(mainFormElement2, "Export", true);
                count++;
                Thread.Sleep(1000);

            } while (exportButton2 == null & count < 10);

            automation.invokeButtonPress(exportButton2);

            Thread.Sleep(500);

            count = 0;
            do
            {
                exportAsComboBox2 = automation.setChildElementByName(mainFormElement2, "Export as", true);
                count++;
                Thread.Sleep(1000);

            } while (exportAsComboBox2 == null & count < 10);


            if (exportAsComboBox2 != null)
            {
                SendKeys.SendWait("{UP}");
                SendKeys.SendWait("{TAB}");
                SendKeys.SendWait(" ");
                okButton2 = automation.setChildElementByName(mainFormElement2, "F2 Ok", true);
                automation.invokeButtonPress(okButton2);
                saveFile(automation, programMethods, reportPath, savedFile);
            }



        }

        private void saveFile2(AutomationMethods automation, Program programMethods, string reportPath, string savedFile)
        {

            Thread.Sleep(1000);


            AutomationElement
                    mainFormElement2,
                    saveWindow2;

            mainFormElement2 = automation.setParentElement("BMIS V3.4.9");
            int loopcount = 0;
            do
            {
                loopcount++;
                saveWindow2 = automation.setChildElementByName(mainFormElement2, "Save As", true);
                Thread.Sleep(500);
            } while (saveWindow2 == null && loopcount < 5);

            if (saveWindow2 != null)
            {
                SendKeys.SendWait(savedFile);
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{ENTER}");
                SendKeys.SendWait(reportPath);
                Thread.Sleep(1000);
                SendKeys.SendWait("{ENTER}");
                Thread.Sleep(1000);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200)
                    ;
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200);
                SendKeys.SendWait("{ENTER}");
                Thread.Sleep(2000);
                SendKeys.SendWait("{LEFT}");
                Thread.Sleep(200);
                SendKeys.SendWait("{ENTER}");

            }


        }

        private void closeBMIS2()
        {

            Thread.Sleep(4000);


            try
            {

                SendKeys.SendWait("%{F4}");
                Thread.Sleep(1000);
                System.Environment.Exit(1);
            }
            catch (Exception ex)
            {

            }

        }



        public void generalErrorMessage()
        {
            MessageBox.Show("LOGIN ERROR", "LOGIN ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }

    }
}
