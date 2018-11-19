using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using PGRemoteRPC;
/// <summary>
/// 預期有的function
/// 輸入測試條件,選擇測試item,執行自動測試
/// 取代Swing , Skew , HS/LP Short Long Write , Read check fucntion
/// 
/// </summary>
namespace P338_Auto_Tool
{
    class Auto_Test_Thread
    {
        public class MIPI_Parameter
        {
            public int lane, vsa, vbp, vfp, vact, hsa, hbp, hfp, hact, bbp, VideoTime;
            public float bitrate, framerate, HS_High = 0.4F, HS_Low = 0, LP_High = 1.2F, LP_Low = 0F, LP_Freq = 18e+6F, D0_Delay, D1_Delay, D2_Delay, D3_Delay, D4_Delay, CLK_Delay
    , hs_prepare, hs_zero, hs_exit, hs_trail, clk_prepare, clk_zero, clk_trail, clk_pre, clk_post, TA_go, T_wakeup;
            public string picture_path;
            public bool video_pulse_mode, video_event_mode, video_burst_mode, video_LP_by_line, video_LP_by_Frame;
            public void Calculate_bitrate()
            {
                bitrate = (((hsa + hbp + hfp + hact) * (vsa + vbp + vfp + vact)) * framerate * bbp) / lane;
            }
        }
        MIPI_Parameter Test_Condition = new MIPI_Parameter();
        MIPI_Parameter PG_Setting_Value = new MIPI_Parameter();
        public string output_path;
        /// <summary>
        /// 從預設Excel格式取得測試條件
        /// </summary>
        /// <param name="filepath"></param>
        public void Get_Test_Condition_From_Excel()
        {
            MIPI_Auto_Test Auto_Control = new MIPI_Auto_Test();
            Auto_Control.Excel_open(output_path, 1);
            Auto_Control.EXcel_sheet_select("outline");
            Test_Condition.bitrate = Convert.ToInt32(Auto_Control.Read_Excel_cell(14, 3));
            Test_Condition.lane = Convert.ToInt32(Auto_Control.Read_Excel_cell(13, 3));
            Test_Condition.hact = Convert.ToInt32(Auto_Control.Read_Excel_cell(7, 3));
            Test_Condition.vact = Convert.ToInt32(Auto_Control.Read_Excel_cell(8, 3));
            Test_Condition.vsa = Convert.ToInt32(Auto_Control.Read_Excel_cell(9, 3));
            Test_Condition.vbp = Convert.ToInt32(Auto_Control.Read_Excel_cell(10, 3));
            Test_Condition.vfp = Convert.ToInt32(Auto_Control.Read_Excel_cell(11, 3));
            Test_Condition.framerate = Convert.ToInt32(Auto_Control.Read_Excel_cell(12, 3));
            Auto_Control.Close_Excel();
        }
        /*
        private int lane, vsa, vbp, vfp, vact, hsa, hbp, hfp, hact , defalut_hsa , defalut_hbp , defalut_hfp , defalut_, defalut_vbp, defalut_vfp;
        private float bitrate, framerate , bitrate_spec;
        */
        ///實驗可能條件:bitrate , lane , res
        List<bool> auto_item = new List<bool>() { false, false, true, false, false };
        public delegate void Thread_Delegate();
        public delegate void Actiming_calculate_delegate();
        public Thread_Delegate td;
        private List<Thread_Delegate> Td_list = new List<Thread_Delegate>();
        //public Thread_Delegate[] tdarray = new Thread_Delegate[3] {Auto_Ac}
        DSITiming DSI_Timing_Cal = new DSITiming();


        public void Run_Item()
        {
            Td_list.Add(new Thread_Delegate(Auto_Swing_Task));
            Td_list.Add(new Thread_Delegate(Auto_Skew_Task));
            Td_list.Add(new Thread_Delegate(Auto_Actiming_Task));
            Td_list.Add(new Thread_Delegate(Auto_VideoMode_Task));
            Td_list.Add(new Thread_Delegate(Auto_HS_Task));
            for (int i = 0; i < auto_item.Count; i++)
            {
                if (auto_item[i])
                {
                    td += Td_list[i];
                }
            }
            if (td != null) td();
        }

        public void Run_Videomode_Test()
        {

        }
        public void Run_Skew_Test()
        {

        }
        public void Run_Swing_Test()
        {

        }
        public void Run_Actiming_Test()
        {

        }
        public void Run_Hs_Test()
        {

        }
        public Auto_Test_Thread()
        {

        }
        public Auto_Test_Thread(string output_path)
        {
            this.output_path = output_path;
        }

        public void Set_File_Path(string file_path)
        {
            this.output_path = file_path;
        }
        public void Set_Test_Bitrate(float bitrate)
        {
            Test_Condition.bitrate = bitrate;
        }
        private void Auto_Swing_Task()
        {
            MIPI_Auto_Test Auto_Control = new MIPI_Auto_Test();
            Auto_Control.Excel_open(output_path, 1);
            Auto_Control.EXcel_sheet_select("HS_Swing");
            float[,] swing_item = new float[,] { { 0.29F, -0.15F }, { 0.14F, 0F }, { 0.59F, 0.07F }, { 0.4F, 0.26F } };
            int x_start = 3;
            int y_start = 19;
            int x_now = x_start;
            int y_now = y_start;
            for (int i = 0; i < swing_item.GetLength(0); i++)
            {
                for (float bitrate = 100; bitrate <= Test_Condition.bitrate; bitrate += 50)
                {
                    //設定PG
                    //送Command
                    //Read back
                    //寫入excel                    
                    Auto_Control.Write_Excel_cell(y_now, x_now, bitrate.ToString() + swing_item[i, 0].ToString() + swing_item[i, 1].ToString());
                    y_now++;
                }
                y_now = y_start;
                x_now++;
            }
            Auto_Control.Save_Excel();
            Auto_Control.Close_Excel();

        }

        private void Auto_Actiming_Task()
        {
            StreamWriter sw = new StreamWriter(@"D:\ttt.txt");
            DSITiming ACtiming_Auto = new DSITiming();
            MIPI_Auto_Test Auto_Control = new MIPI_Auto_Test();
            Auto_Control.Excel_open(output_path, 1);
            Auto_Control.EXcel_sheet_select("AC timing");
            int x_start = 18, y_start = 5; //驗證報告actiming部分表格的起始點
            for (float bitrate_now = 100; bitrate_now < Test_Condition.bitrate; bitrate_now += 100)
            {
                ACtiming_Auto.set_bitrate(bitrate_now);
                sw.Write("defalut\t" + ACtiming_Auto.get_timing_setting());
                //clk post max
                ACtiming_Auto.get_clk_post_min_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                //send command
                //確認回傳值

                sw.Write("clk_post_max\t" + ACtiming_Auto.get_timing_setting());
                //clk pre min
                ACtiming_Auto.get_clk_pre_min_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("clk_pre_min\t" + ACtiming_Auto.get_timing_setting());
                //clk prepare min
                ACtiming_Auto.get_clk_prepare_min_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("clk_prepare_min\t" + ACtiming_Auto.get_timing_setting());
                //clk prepare max
                ACtiming_Auto.get_clk_prepare_max_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("clk_prepare_max\t" + ACtiming_Auto.get_timing_setting());
                //clk trail min
                ACtiming_Auto.get_clk_trail_min_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("clk_trail_min\t" + ACtiming_Auto.get_timing_setting());
                //clk prepare + zero min
                ACtiming_Auto.get_clk_prepare_zero_min_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("clk_prepare+clk_zero_min\t" + ACtiming_Auto.get_timing_setting());
                //clk trail max
                ACtiming_Auto.get_clk_trail_max_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("clk_trail_max\t" + ACtiming_Auto.get_timing_setting());
                //data trail max
                ACtiming_Auto.get_hs_trail_max_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("hs_trail_max\t" + ACtiming_Auto.get_timing_setting());
                //hs prepare min
                ACtiming_Auto.get_hsprepare_min_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("hs_prepare_min\t" + ACtiming_Auto.get_timing_setting());
                //hs prepare max
                ACtiming_Auto.get_hsprepare_max_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("hs_prepare_max\t" + ACtiming_Auto.get_timing_setting());
                //hs prepare + zero min
                ACtiming_Auto.get_hsprepare_zero_min_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("hs_prepare+hs_zero\t" + ACtiming_Auto.get_timing_setting());
                //hs trail min       
                ACtiming_Auto.get_hs_trail_min_setting();
                //Send_Timing_To_PG(ACtiming_Auto);
                sw.Write("hs_trail_min\t" + ACtiming_Auto.get_timing_setting());
            }
            sw.Close();
        }

        private void Send_Timing_To_PG(DSITiming ACtiming)
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;
            client.PGRemoteCmd(RPCCmds.START_EDIT_CONFIG, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_LP_FREQ, (float)18e+6, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_FREQ, PG_Setting_Value.bitrate, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.END_EDIT_CONFIG, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_USE_SYSTEM_COMPUTED_TIMING, 1, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_USE_SYSTEM_COMPUTED_TIMING, 0, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_HS_PREPARE, ACtiming.hs_prepare, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_HS_ZERO, ACtiming.hs_zero, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_HS_EXIT, ACtiming.hs_exit, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_HS_TRAIL, ACtiming.hs_trail, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_PREPARE, ACtiming.clk_prepare, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_ZERO, ACtiming.clk_zero, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_TRAIL, ACtiming.clk_trail, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_PRE, ACtiming.clk_pre, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_POST, ACtiming.clk_post, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_TA_GO, ACtiming.TA_go, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_WAKEUP, ACtiming.T_wakeup, ref errMsg, ref statusMsg);
            client.Disconnect(true);
        }

        private void Auto_HS_Task()
        {
            MIPI_Auto_Test Auto_Control = new MIPI_Auto_Test();
            Auto_Control.Excel_open(output_path, 1);
            Auto_Control.EXcel_sheet_select("HS performace");
            int x_start = 18, y_start = 5; //驗證報告actiming部分表格的起始點
        }

        private void Auto_Skew_Task()
        {
            MIPI_Auto_Test Auto_Control = new MIPI_Auto_Test();
            Auto_Control.Excel_open(output_path, 1);
            Auto_Control.EXcel_sheet_select("Skew");
            int x_start = 18;
            int y_start = 3;
            int x_now = x_start;
            int y_now = y_start;
            int ui_step = 100;
            float ui = 1 / (Test_Condition.bitrate);
            float ui_step_unit = ui / ui_step;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < ui_step; j++)
                {
                    /*設定條件*/
                    /*送出Data*/
                    /*取得回傳值*/
                    /*寫入excel*/
                    Auto_Control.Write_Excel_cell(y_now, x_now + j, (ui_step_unit * j).ToString());
                }
                y_now++;
                x_now = x_start;
            }
            Auto_Control.Save_Excel();
            Auto_Control.Close_Excel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blanking_type">1:lp every frame 2:lp every line</param>
        /// <param name="sync_mode">1:pulse mode 2:event mode 3: burst mode</param>
        public void Set_VideoMode_Type(int blanking_type, int sync_mode)
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;
            if (blanking_type == 1)
            {
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HBPORCH_BLANKING_MODE, 2, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HFPORCH_BLANKING_MODE, 2, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HSYNC_BLANKING_MODE, 2, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_VERTICAL_BLANKING_MODE, 1, ref errMsg, ref statusMsg);
            }
            else if (blanking_type == 2)
            {
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HBPORCH_BLANKING_MODE, 1, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HFPORCH_BLANKING_MODE, 1, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HSYNC_BLANKING_MODE, 1, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_VERTICAL_BLANKING_MODE, 1, ref errMsg, ref statusMsg);
            }
            else return;
            if (sync_mode == 1)
            {
                client.PGRemoteCmd(RPCCmds.SET_TIMING_ENABLE_DSI_BURST_MODE, false, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_ENABLE_DSI_PULSE_MODE, true, ref errMsg, ref statusMsg);
            }
            else if (sync_mode == 2)
            {
                client.PGRemoteCmd(RPCCmds.SET_TIMING_ENABLE_DSI_BURST_MODE, false, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_ENABLE_DSI_PULSE_MODE, false, ref errMsg, ref statusMsg);
            }
            else if (sync_mode == 3)
            {
                client.PGRemoteCmd(RPCCmds.SET_TIMING_ENABLE_DSI_BURST_MODE, true, ref errMsg, ref statusMsg);
            }
            else return;


            client.Disconnect(true);
        }
        private void Auto_VideoMode_Task()
        {
            bool pulse_mode = Test_Condition.video_pulse_mode;
            bool event_mode = Test_Condition.video_event_mode;
            bool burst_mode = Test_Condition.video_burst_mode;
            bool lp_every_line = Test_Condition.video_LP_by_line;
            bool lp_every_frame = Test_Condition.video_LP_by_Frame;
            for (int i = 0; i < 2; i++)
            {
                if (lp_every_frame)
                {
                    if (pulse_mode)
                    {
                        Set_VideoMode_Type(1, 1);
                        Video_Auto_thread(1);
                    }
                    if (event_mode)
                    {
                        Set_VideoMode_Type(1, 2);
                        Video_Auto_thread(3);
                    }
                    if (burst_mode)
                    {
                        Set_VideoMode_Type(1, 3);
                        Video_Auto_thread(5);
                    }
                    lp_every_frame = false;
                }
                if (lp_every_line)
                {
                    if (pulse_mode)
                    {
                        Set_VideoMode_Type(2, 1);
                        Video_Auto_thread(7);
                    }
                    if (event_mode)
                    {
                        Set_VideoMode_Type(2, 2);
                        Video_Auto_thread(9);
                    }
                    if (burst_mode)
                    {
                        Set_VideoMode_Type(2, 3);
                        Video_Auto_thread(11);
                    }
                }
            }

            //MIPI_Parameter initil_porch = new MIPI_Parameter();
            //Video_Auto_thread();
        }
        public void Set_Test_Item(List<bool> items)
        {
            this.auto_item = items;
        }

        private void write_to_txt()
        {

        }

        public int Video_BR_Analysis()
        {
            int count;
            int x_start = 2, y_start = 1;
            bool temp1 = false, temp2 = false, temp3 = false;
            List<string> Video_Reslut = new List<string>();
            MIPI_Auto_Test Auto_Control = new MIPI_Auto_Test();
            Auto_Control.Excel_open(output_path, 1);
            Auto_Control.EXcel_sheet_select("Video mode RAW");
            Video_Reslut = Auto_Control.Read_Excel_Column(x_start, y_start);
            for (count = 0; count < Video_Reslut.Count; count++)
            {
                temp1 = temp2;
                temp2 = temp3;
                temp3 = (Video_Reslut[count] == "pass") ? false : true;
                if (temp1 & temp2 & temp3)
                {
                    Auto_Control.Close_Excel();
                    return count - 2;
                }
            }
            Auto_Control.Close_Excel();
            return count;
        }
        private void Set_AcTiming()
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;
            client.PGRemoteCmd(RPCCmds.START_EDIT_CONFIG, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_LP_FREQ, (float)18e+6, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_FREQ, PG_Setting_Value.bitrate, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.END_EDIT_CONFIG, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_USE_SYSTEM_COMPUTED_TIMING, 1, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_USE_SYSTEM_COMPUTED_TIMING, 0, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_HS_PREPARE, PG_Setting_Value.hs_prepare, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_HS_ZERO, PG_Setting_Value.hs_zero, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_HS_EXIT, PG_Setting_Value.hs_exit, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_HS_TRAIL, PG_Setting_Value.hs_trail, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_PREPARE, PG_Setting_Value.clk_prepare, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_ZERO, PG_Setting_Value.clk_zero, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_TRAIL, PG_Setting_Value.clk_trail, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_PRE, PG_Setting_Value.clk_pre, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_CLK_POST, PG_Setting_Value.clk_post, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_TA_GO, PG_Setting_Value.TA_go, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_DPHY_PARAMETER, RPCDefs.DPHY_PARAM_WAKEUP, PG_Setting_Value.T_wakeup, ref errMsg, ref statusMsg);
            client.Disconnect(true);
        }

        public void Set_DPHY_Swing(MIPI_Parameter MIPI_Setting)
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;
            client.PGRemoteCmd(RPCCmds.START_EDIT_CONFIG, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_HIGH_VOLT, 0, MIPI_Setting.HS_High, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_LOW_VOLT, 0, MIPI_Setting.HS_High, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_LP_HIGH_VOLT, 0, MIPI_Setting.LP_High, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_LP_LOW_VOLT, 0, MIPI_Setting.LP_Low, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.END_EDIT_CONFIG, ref errMsg, ref statusMsg);
            client.Disconnect(true);
        }

        public void Set_DPHY_Skew(MIPI_Parameter MIPI_Setting)
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;
            client.PGRemoteCmd(RPCCmds.START_EDIT_CONFIG, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_DELAY, 0, MIPI_Setting.D0_Delay * 1e-12, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_DELAY, 1, MIPI_Setting.D1_Delay * 1e-12, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_DELAY, 2, MIPI_Setting.D2_Delay * 1e-12, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_DELAY, 3, MIPI_Setting.D3_Delay * 1e-12, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_HS_DELAY, 4, MIPI_Setting.CLK_Delay * 1e-12, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.END_EDIT_CONFIG, ref errMsg, ref statusMsg);
            client.Disconnect(true);
        }

        public void Video_mode_Setting(int hact, int hsa, int hbp, int hfp, int vact, int vsa, int vbp, int vfp)
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;
            client.PGRemoteCmd(RPCCmds.SET_TIMING_HSYNC, hsa, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_TIMING_HBPORCH, hbp, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_TIMING_HFPORCH, hfp, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_TIMING_HACTIVE, hact, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_TIMING_VSYNC, vsa, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_TIMING_VBPORCH, vbp, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_TIMING_VFPORCH, vfp, ref errMsg, ref statusMsg);
            client.PGRemoteCmd(RPCCmds.SET_TIMING_VACTIVE, vact, ref errMsg, ref statusMsg);
            client.Disconnect(true);
        }
        public void Set_Video_Auto_Condition(int lane, float bitrate, int hact, int hsa, int hbp, int hfp, int vact, int vsa, int vbp, int vfp, int bbp, float framerate, string Picture_path)
        {
            Test_Condition.lane = lane;
            Test_Condition.bitrate = bitrate * 1000000F;
            Test_Condition.hact = hact;
            Test_Condition.hsa = hsa;
            Test_Condition.hbp = hbp;
            Test_Condition.hfp = hfp;
            Test_Condition.vact = vact;
            Test_Condition.vsa = vsa;
            Test_Condition.vbp = vbp;
            Test_Condition.vfp = vfp;
            Test_Condition.bbp = bbp;
            Test_Condition.framerate = framerate;
            Test_Condition.picture_path = Picture_path;
            Test_Condition.VideoTime = 5000;
            Test_Condition.video_LP_by_Frame = true;
            Test_Condition.video_LP_by_line = true;
            Test_Condition.video_burst_mode = true;
            Test_Condition.video_pulse_mode = true;
            Test_Condition.video_event_mode = true;
        }

        public void Set_Video_Auto_Condition(int lane, float bitrate, int hact, int hsa, int hbp, int hfp, int vact, int vsa, int vbp, int vfp, int bbp, float framerate, string Picture_path, int videotime)
        {
            Test_Condition.lane = lane;
            Test_Condition.bitrate = bitrate * 1000000F;
            Test_Condition.hact = hact;
            Test_Condition.hsa = hsa;
            Test_Condition.hbp = hbp;
            Test_Condition.hfp = hfp;
            Test_Condition.vact = vact;
            Test_Condition.vsa = vsa;
            Test_Condition.vbp = vbp;
            Test_Condition.vfp = vfp;
            Test_Condition.bbp = bbp;
            Test_Condition.framerate = framerate;
            Test_Condition.picture_path = Picture_path;
            Test_Condition.VideoTime = videotime;
        }

        public void Video_Auto_thread(int start_X)
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;
            MIPI_Auto_Test Auto_Control = new MIPI_Auto_Test();
            int excel_X = start_X, excel_Y = 1;
            Auto_Control.Excel_open(output_path, 2);
            PG_Setting_Value.lane = Test_Condition.lane;
            PG_Setting_Value.hsa = Test_Condition.hsa;
            PG_Setting_Value.hbp = Test_Condition.hbp;
            PG_Setting_Value.hfp = Test_Condition.hfp;
            PG_Setting_Value.hact = Test_Condition.hact;
            PG_Setting_Value.vsa = Test_Condition.vsa;
            PG_Setting_Value.vbp = Test_Condition.vbp;
            PG_Setting_Value.vfp = Test_Condition.vfp;
            PG_Setting_Value.vact = Test_Condition.vact;
            PG_Setting_Value.framerate = Test_Condition.framerate;
            PG_Setting_Value.bbp = Test_Condition.bbp;

            PG_Setting_Value.Calculate_bitrate();
            while (PG_Setting_Value.bitrate < Test_Condition.bitrate)
            {
                client.PGRemoteCmd(RPCCmds.START_EDIT_CONFIG, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_LP_FREQ, (float)18e+6, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_HS_FREQ, (PG_Setting_Value.bitrate) / 2 + 1, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.END_EDIT_CONFIG, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HSYNC, PG_Setting_Value.hsa, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HBPORCH, PG_Setting_Value.hbp, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HFPORCH, PG_Setting_Value.hfp, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_HACTIVE, PG_Setting_Value.hact, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_VSYNC, PG_Setting_Value.vsa, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_VBPORCH, PG_Setting_Value.vbp, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_VFPORCH, PG_Setting_Value.vfp, ref errMsg, ref statusMsg);
                client.PGRemoteCmd(RPCCmds.SET_TIMING_VACTIVE, PG_Setting_Value.vact, ref errMsg, ref statusMsg);
                client.MIPICmd(RPCDefs.PACKED_PIXEL_STREAM_888, 0, false, RPCDefs.DT_HS, 0, 1, 0, 0, Test_Condition.picture_path, null, ref errMsg, ref statusMsg);
                client.MIPICmd(RPCDefs.BTA, 0, false, RPCDefs.DT_LP, 0, 0, 0, 0, "", null, ref errMsg, ref statusMsg);
                Thread.Sleep(5000);
                client.MIPICmd(RPCDefs.BTA, 0, false, RPCDefs.DT_LP, 0, 0, 0, 0, "", null, ref errMsg, ref statusMsg);
                client.PGRemoteQuery(RPCCmds.GET_DUT_RESPONSE, 0, ref DUTResp, ref errMsg, ref statusMsg);
                Auto_Control.Write_Excel_cell(excel_Y, excel_X, "HFP " + PG_Setting_Value.hfp + "HBP " + PG_Setting_Value.hbp + "HSA " + PG_Setting_Value.hsa + "Bit Rate " + (PG_Setting_Value.bitrate) / 1000000);
                Auto_Control.Write_Excel_cell(excel_Y, excel_X + 1, BitConverter.ToString(DUTResp));
                excel_Y++;
                client.PGRemoteCmd(RPCCmds.PG_ABORT, ref errMsg, ref statusMsg);
                PG_Setting_Value.hsa += 16;
                PG_Setting_Value.hbp += 16;
                PG_Setting_Value.hfp += 16;
                PG_Setting_Value.Calculate_bitrate();
            }
            client.Disconnect(true);
            Auto_Control.Save_Excel();
            Auto_Control.Close_Excel();
        }

        public void SEND_Video_mode(string file_path)
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;
            client.MIPICmd(RPCDefs.PACKED_PIXEL_STREAM_888, 0, false, RPCDefs.DT_HS, 0, 1, 0, 0, file_path, null, ref errMsg, ref statusMsg);
            client.Disconnect(true);
        }

        public void Send_Split_RPC(string file_path)
        {
            PGRemoteRPCClient client = new PGRemoteRPCClient();
            int rc = client.Connect("", 2799);
            string errMsg = "";
            string statusMsg = "";
            byte[] DUTResp = new byte[0];
            if (rc < 0) return;

            //StreamWriter sw = new StreamWriter(picture_path.FileName);
            StreamReader sr = new StreamReader(file_path);

            string line = string.Empty;
            int temp = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("/"))
                {
                    if (File.Exists(temp.ToString()))
                    {
                        client.MIPICmd(RPCDefs.RPC_SCRIPT, 0, false, RPCDefs.DT_HS, 0, 0, 0, 0, temp.ToString(), null, ref errMsg, ref statusMsg);
                        client.PGRemoteQuery(RPCCmds.GET_DUT_RESPONSE, 0, ref DUTResp, ref errMsg, ref statusMsg);
                        try
                        {
                            File.Delete(temp.ToString());
                        }
                        catch (System.IO.IOException error)
                        {
                            Console.WriteLine(error.ToString());
                            return;
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
            }
        }

        public class P338_Control
        {
            public void P338_Loop_Command(bool a)
            {
                PGRemoteRPCClient client = new PGRemoteRPCClient();
                int rc = client.Connect("", 2799);
                string errMsg = "";
                string statusMsg = "";
                byte[] DUTResp = new byte[0];
                if (rc < 0) return;
                client.PGRemoteCmd(RPCCmds.SET_OPTION, RPCDefs.OPT_LOOP_NON_VIDEO_COMMANDS, a, ref errMsg, ref statusMsg);
                client.Disconnect(true);
            }

            public void Eotp_Switch(bool a)
            {
                PGRemoteRPCClient client = new PGRemoteRPCClient();
                int rc = client.Connect("", 2799);
                string errMsg = "";
                string statusMsg = "";
                byte[] DUTResp = new byte[0];
                if (rc < 0) return;
                client.PGRemoteCmd(RPCCmds.SET_OPTION, RPCDefs.OPT_ENABLE_EOT_PKTS, a, ref errMsg, ref statusMsg);
                client.Disconnect(true);
            }

            public void Clock_Alway_Switch(bool a)
            {
                PGRemoteRPCClient client = new PGRemoteRPCClient();
                int rc = client.Connect("", 2799);
                string errMsg = "";
                string statusMsg = "";
                byte[] DUTResp = new byte[0];
                if (rc < 0) return;
                client.PGRemoteCmd(RPCCmds.SET_OPTION, RPCDefs.OPT_CLOCK_ON_OFF_EACH_COMMAND, a, ref errMsg, ref statusMsg);
                client.Disconnect(true);
            }

            public void PG_Stop()
            {
                PGRemoteRPCClient client = new PGRemoteRPCClient();
                int rc = client.Connect("", 2799);
                string errMsg = "";
                string statusMsg = "";
                byte[] DUTResp = new byte[0];
                if (rc < 0) return;
                client.PGRemoteCmd(RPCCmds.PG_ABORT, ref errMsg, ref statusMsg);
                client.Disconnect(true);
            }
        }

    }
}
