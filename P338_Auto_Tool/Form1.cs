using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using PGRemoteRPC;
using System.IO;

namespace P338_Auto_Tool
{
    public partial class Form1 : Form
    {
        
        public string output_path;        
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_open_output_path_Click(object sender, EventArgs e)
        {
            OpenFileDialog openOutputData = new OpenFileDialog();
            if (openOutputData.ShowDialog() != DialogResult.OK) return;
            output_path = openOutputData.FileName;
            OpenFileDialog picture_path = new OpenFileDialog();
            if (picture_path.ShowDialog() != DialogResult.OK) return;
            Auto_Test_Thread ATT = new Auto_Test_Thread(output_path);
            ATT.Set_Video_Auto_Condition(4, 1100, 1080, 60, 60, 60, 2160, 8, 8, 8, 24, 60, picture_path.FileName);
            Thread tt = new Thread(ATT.Run_Item);
            tt.Start();
            //ATT.Run_Item();
            //ATT.Auto_Swing_Task();


        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = true;
                checkBox3.Checked = true;
                checkBox4.Checked = true;
                checkBox5.Checked = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ///OpenFileDialog opdata = new OpenFileDialog();
            Auto_Test_Thread ATT = new Auto_Test_Thread();
            Auto_Test_Thread.P338_Control PG_Control = new Auto_Test_Thread.P338_Control();
            PG_Control.P338_Loop_Command(false);
            PG_Control.Clock_Alway_Switch(false);
            PG_Control.Eotp_Switch(false);
            textBox1.Text = "done";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog picture_path = new OpenFileDialog();
            if (picture_path.ShowDialog() != DialogResult.OK) return;
            Auto_Test_Thread ATT = new Auto_Test_Thread();
            Auto_Test_Thread.P338_Control PG_Control = new Auto_Test_Thread.P338_Control();
            /*
            ATT.SEND_Video_mode(picture_path.FileName);
            PG_Control.PG_Stop();
            Thread.Sleep(5000);
            ATT.Video_mode_Setting(1080, 30, 30, 30, 2160, 8, 9, 10);
            ATT.SEND_Video_mode(picture_path.FileName);
            */
            ATT.Set_Video_Auto_Condition(4 , 1100, 1080, 60, 60, 60, 2160, 8, 8, 8, 24 ,60,picture_path.FileName);
            ATT.Video_Auto_thread(1);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int Y_Start = 5;
            int Y_now = 1;
            Y_now = Y_Start;
            OpenFileDialog openOutputData = new OpenFileDialog();
            if (openOutputData.ShowDialog() != DialogResult.OK) return;
            output_path = openOutputData.FileName;
            MIPI_Auto_Test Auto_Control = new MIPI_Auto_Test();
            Auto_Control.Excel_open(output_path, 1);
            Auto_Control.EXcel_sheet_select("AC timing");
            //設定條件
            //送Data
            
            for(int i = Y_Start; i < 180+Y_Start; i++)
            {
                Auto_Control.Write_Excel_cell(i, 19, "123");
            }            
            Auto_Control.Save_Excel();
            Auto_Control.Close_Excel();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog picture_path = new OpenFileDialog();
            if (picture_path.ShowDialog() != DialogResult.OK) return;
            Auto_Test_Thread ATT = new Auto_Test_Thread();
            ATT.Set_File_Path(picture_path.FileName);
            ATT.Get_Test_Condition_From_Excel();
            textBox1.Text = ATT.Video_BR_Analysis().ToString();
            //Thread tt = new Thread(ATT.Run_Item);
            ATT.Run_Item();
            textBox1.Text = "done";
        }

        public void rpc_function_tset(string path)
        {
            int rc;
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            rc = client.Connect("", 2799);
            if (rc < 0)
                textBox1.Text = " can't connect to PGReomte";

            //StreamWriter sw = new StreamWriter(picture_path.FileName);
            StreamReader sr = new StreamReader(path);

            string line = string.Empty;
            int temp = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("/"))
                {
                    if (File.Exists(temp.ToString() + ".txt"))
                    {
                        client.MIPICmd(RPCDefs.RPC_SCRIPT, 0, false, RPCDefs.DT_HS, 0, 0, 0, 0, System.IO.Directory.GetCurrentDirectory() + "\\" + temp.ToString() + ".txt", null, ref errMsg, ref statusMsg);
                        client.PGRemoteQuery(RPCCmds.GET_DUT_RESPONSE, 0, ref DUTResp, ref errMsg, ref statusMsg);
                        textBox1.Text = BitConverter.ToString(DUTResp);
                        try
                        {
                            File.Delete(temp.ToString()+ ".txt");
                        }
                        catch (System.IO.IOException error)
                        {
                            Console.WriteLine(error.ToString());
                            return;
                        }
                    }

                    
                    temp++;
                }

                else
                {
                    StreamWriter sw = new StreamWriter(temp.ToString() + ".txt", true);
                    sw.WriteLine(line);
                    sw.Close();
                }

                }
            sr.Close();
            }
        

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog rpc_path = new OpenFileDialog();
            if (rpc_path.ShowDialog() != DialogResult.OK) return;
            rpc_function_tset(rpc_path.FileName);
        }
    }
}
