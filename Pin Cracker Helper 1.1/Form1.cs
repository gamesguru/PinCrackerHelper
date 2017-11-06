using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pin_Cracker_Helper_1._00
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Declarations
        MouseFunctions pMouse = new MouseFunctions();
        Process PinCrackerProcess = null;
        Process MapleProcess = null;
        int out2;
        ProcessMemoryReader pReader = new ProcessMemoryReader();
        string maplePath;
        string pinPath;
        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            UInt32 size
            );

        [DllImport("user32.dll", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            RefreshProcs();
            if (MapleProcess != null && PinCrackerProcess != null) //both are open
            {
                label1.Text = "Both found and injected!";
                checkBox1.Enabled = true;
                System.Threading.Thread.Sleep(10);
                pReader.ReadProcess = MapleProcess;
                pReader.OpenProcess();
                byte[] dialogValue = pReader.ReadProcessMemory((IntPtr)0x8530B0, 4, out out2);
                if (BitConverter.ToInt32(dialogValue, 0) == 0) //maple's at a "connection ended" or another dialog
                {
                    timer2.Enabled = false;
                    ReloadMaple();
                    timer2.Enabled = true;
                }
            }
            else
            {
                label1.Text = "Maple/Cracker not found.";
                checkBox1.Checked = false;
            }
            timer1.Enabled = true;
        }

        void ReloadMaple()
        {
            RefreshProcs();
            maplePath = MapleProcess.MainModule.FileName;
            pinPath = PinCrackerProcess.MainModule.FileName;
            MapleProcess.Kill(); PinCrackerProcess.Kill(); //kills them both
            Process.Start(maplePath);
            System.Threading.Thread.Sleep(2000);
            Process.Start(pinPath);
            System.Threading.Thread.Sleep(7000);
            RefreshProcs();
            MapleProcess.CloseMainWindow(); //gets rid of the PLAY menu
            System.Threading.Thread.Sleep(30000);
            SwitchToThisWindow(PinCrackerProcess.MainWindowHandle, true);
            System.Threading.Thread.Sleep(5000);
            //pin cracker is now active
            SendKeys.SendWait("{F2}");
            System.Threading.Thread.Sleep(500);
            //OK menu now active on cracker
            //bug where it is not active is addressed here
            pMouse.MousePos(524, 544);
            System.Threading.Thread.Sleep(50);
            pMouse.LeftClick();
            System.Threading.Thread.Sleep(500);
            SendKeys.SendWait("{ENTER}");
            //cracking process is now active again
        }

        private void RefreshProcs()
        {
            Process[] procs = Process.GetProcesses();
            foreach (Process p in procs)
            {
                if (p.ProcessName.ToLower() == "pin cracker")
                {
                    PinCrackerProcess = p;
                    break;
                }
                else
                    PinCrackerProcess = null;
            }

            foreach (Process p in procs)
            {
                if (p.ProcessName.ToLower() == "maplestory")
                {
                    MapleProcess = p;
                    break;
                }
                else
                    MapleProcess = null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form.CheckForIllegalCrossThreadCalls = false;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
                e.Handled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "") textBox1.Text = "0";
            Properties.Settings.Default.idleTime = Convert.ToInt16(textBox1.Text);
            Properties.Settings.Default.Save();
        }

        bool froze = false;

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            RefreshProcs();
            if (MapleProcess != null && PinCrackerProcess != null) //both are open
            {
                label1.Text = "Both found and injected!";
                System.Threading.Thread.Sleep(10);
                pReader.ReadProcess = MapleProcess;
                pReader.OpenProcess();
                int[] dv = new int[80];
                for (int i = 0; i < 80; i++)
                {
                    dv[i] = BitConverter.ToInt32(pReader.ReadProcessMemory((IntPtr)0x8530B0, 4, out out2), 0);
                    System.Threading.Thread.Sleep(100);
                }
                for (int i = 0; i < 80; i++)
                {
                    for (int s = 0; s < 80; s++)
                    {
                        if (dv[i] == dv[s] && dv[i] != 0)//all are equal
                        {
                            froze = true;
                        }
                        else
                        {
                            froze = false;
                            break;
                        }
                    }
                }
            }
            if (froze)
                ReloadMaple();
            timer2.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            RefreshProcs();
            if (MapleProcess == null || PinCrackerProcess == null)
            {
                if(checkBox1.Checked)
                MessageBox.Show("Error: Maple or Pin Cracker not detected!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkBox1.Checked = false;
                timer1.Enabled = false;
                timer2.Enabled = false;
            }
            else
            {
                if (checkBox1.Checked)
                {
                    MessageBox.Show("You have 10 seconds to click ''Start'' on the Pin Cracker. Hurry!", "Click start on Pin Cracker.");
                    System.Threading.Thread.Sleep(10000);
                }
                timer1.Enabled = checkBox1.Checked;
                timer2.Enabled = checkBox1.Checked;
            }
        }

    }
}
