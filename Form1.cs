﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CESDK;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace My_Own_Scanner_Attempt
{
    public partial class Form1 : Form
    {

        MemScan ms;
        FoundList fl;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Yummy!");
            GC.Collect();
        }

        private void MemScanDone(object sender)
        {
            //called from CE's main UI thread. Problematic if the form was created using a new thread
            if (this.InvokeRequired)
            {
                this.BeginInvoke(((MemScan)sender).OnScanDone, sender);
            }
            else
            {
                int count;
                fl.Initialize();

                count = fl.Count;
                listView1.VirtualListSize = count;

                scanButton.Enabled = true;
                resetButton.Enabled = true;
                progressBar1.Value = 0;
            }

        }

        private void MemScanGuiUpdate(object sender, UInt64 TotalAddressesToScan, UInt64 CurrentlyScanned, UInt64 ResultsFound)
        {
            //called from CE's main UI thread. Problematic if the form was created using a new thread
            if (this.InvokeRequired)
            {
                this.BeginInvoke(((MemScan)sender).OnGuiUpdate, sender, TotalAddressesToScan, CurrentlyScanned, ResultsFound);
            }
            else
            {
                if (TotalAddressesToScan > 0)
                {
                    int percentage = (int)((double)(CurrentlyScanned / TotalAddressesToScan) * 100);
                    progressBar1.Value = percentage;
                }
                else
                    progressBar1.Value = 0;
            }
        }

        private VarTypes SelectedVarType()
        {
            switch (valueTypeComboBox.SelectedIndex)
            {
                case 0: return VarTypes.vtByte;
                case 1: return VarTypes.vtWord;
                case 2: return VarTypes.vtDword;
                case 3: return VarTypes.vtQword;
                case 4: return VarTypes.vtSingle;
                case 5: return VarTypes.vtDouble;
                case 6: return VarTypes.vtString;
                case 7: return VarTypes.vtByteArray;
                default:
                    return VarTypes.vtDword;

            }

        }

        private void scanButton_Click(object sender, EventArgs e)
        {
            ms.Scan(new ScanParameters
            {
                Value = valueTextBox.Text,
                VarType = SelectedVarType()
            });
            scanButton.Enabled = false;
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            listView1.VirtualListSize = 0;
            ms.Reset();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                ms = new MemScan();

                ms.OnGuiUpdate = MemScanGuiUpdate;
                ms.OnScanDone = MemScanDone;

                fl = new FoundList(ms);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            valueTypeComboBox.SelectedIndex = 2;
            // listView1.VirtualListSize = 10;
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.Item == null)
            {
                e.Item = new ListViewItem();
                e.Item.Text = fl.GetAddress(e.ItemIndex); //  "weee"+e.ItemIndex;
                e.Item.SubItems.Add(fl.GetValue(e.ItemIndex));
            }

        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            //you can also directly access lua without writing a wrapper first
            CESDKLua lua = CESDK.CESDK.currentPlugin.sdk.lua;
            int ProcessID = 0;
            IntPtr Handle = (IntPtr)0;

            lua.GetGlobal("rpcs3");
            if (lua.IsFunction(-1))
            {
                lua.PCall(0, 1);
                ProcessID = (int)lua.ToInteger(-1);
            }
            else
                MessageBox.Show("Failure getting the ProcessID");

            lua.Pop(1);

            lua.GetGlobal("rpcs3");
            if (lua.IsFunction(-1))
            {
                lua.PCall(0, 1);
                Handle = (IntPtr)lua.ToInteger(-1);
            }
            else
                MessageBox.Show("Failure getting the ProcessHandle");

            lua.Pop(1);

            MessageBox.Show("Processid=" + ProcessID + " Handle=" + Handle);
        }
    }
}
