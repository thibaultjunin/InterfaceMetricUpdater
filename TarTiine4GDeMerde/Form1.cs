using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TarTiine4GDeMerde
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            log = new EventLog("_4GDeMerde");
            log.Source = "Service_4G";
        }

        private Boolean using4G = false;
        private Boolean usingADSL = false;
        private EventLog log;

        private void Form1_Load(object sender, EventArgs e)
        {
            log.WriteEntry("Starting service", EventLogEntryType.Information);
            clock.Start();
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            log.WriteEntry("Stopping service", EventLogEntryType.Information);
            clock.Stop();
        }

        private void clock_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, 20, 45, 0);
            DateTime end = new DateTime(now.Year, now.Month, now.Day, 22, 15, 0);

            if (start <= now && now <= end)
            {
                usingADSL = false;
                if (!using4G)
                {
                    // Switch to 4G
                    using4G = true;
                    changeMetric(Properties.Settings.Default.interface_4g, 1);
                    changeMetric(Properties.Settings.Default.interface_adsl, 2);
                }

            }
            else
            {
                using4G = false;
                if (!usingADSL)
                {
                    // Switch to ADSL
                    usingADSL = true;
                    changeMetric(Properties.Settings.Default.interface_4g, 2);
                    changeMetric(Properties.Settings.Default.interface_adsl, 1);
                }
            }
        }

        private void changeMetric(String interfaceId, int metricValue)
        {
            ProcessStartInfo processStart = new ProcessStartInfo();
            processStart.FileName = @"powershell.exe";
            processStart.Arguments = "& {Set-NetIPInterface -InterfaceIndex \"" + interfaceId + "\" -InterfaceMetric \"" + metricValue + "\"}";
            processStart.UseShellExecute = false;
            processStart.CreateNoWindow = true;
            processStart.RedirectStandardOutput = true;
            processStart.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = processStart;
            process.Start();

            process.WaitForExit();

            log.WriteEntry(process.StandardOutput.ReadToEnd(), EventLogEntryType.Information);
            log.WriteEntry(process.StandardError.ReadToEnd(), EventLogEntryType.Error);
        }
    }
}
