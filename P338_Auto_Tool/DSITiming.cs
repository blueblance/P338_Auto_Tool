using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P338_Auto_Tool
{
    class DSITiming
    {
        //Auto_Test_Thread MIPI_Strcut = new Auto_Test_Thread();
        Auto_Test_Thread.MIPI_Parameter MIPI_Parameter = new Auto_Test_Thread.MIPI_Parameter();
        public float hs_prepare, hs_zero, hs_exit, hs_trail, clk_prepare, clk_zero, clk_trail, clk_pre, clk_post, bitrate , TA_go, T_wakeup;
        private float clk_post_min, clk_pre_min, clk_prepare_min, clk_prepare_max, clk_trail_min, clk_zero_min, eot_clk_max, eot_data_max, hs_prepare_min,
     hs_prepare_max, hs_zero_min, hs_trail_min, hs_prepare_zero_min, clk_prepare_zero_min;
        public float typ_hs_prepare, typ_hs_zero, typ_hs_exit, typ_hs_trail, typ_clk_prepare, typ_clk_zero, typ_clk_trail, typ_clk_pre, typ_clk_post;
        public int lane_num , hbp, hfp, hsa, hact, vbp, vfp, vsa, vact, skew_0, skew_1, skew_2, skew_3, skew_clk;
        bool skew_auto, actiming_auto;
        public double frame;
        float p338_timing_step;

        /// <summary>
        /// input bitrate
        /// </summary>
        /// <param name="bitrate">Unit = Mbps</param>
        public DSITiming(float bitrate)
        {                
            skew_auto = true;
            actiming_auto = true;
            this.bitrate = bitrate;
            p338_step();
            timging_cal();
        }

        public DSITiming()
        {
            lane_num = 4;
            bitrate = 1000;
            hbp = hfp = hsa = 100;
            vbp = vfp = vsa = 16;
            hact = 1080;
            vact = 1920;
        }
        public void set_bitrate(float bitrate)
        {
            this.bitrate = bitrate;
            p338_step();
            timging_cal();
        }

        public void set_lane_num(int lane)
        {
            this.lane_num = lane;
        }


        private void p338_step()
        {
            p338_timing_step = 80 / (bitrate / 100);
        }

        private void timging_cal()
        {
            ///計算各bitrate下actiming的極限值
            clk_post_min = 60 + 52 * ((1 / bitrate) * 1000);
            clk_pre_min = 8;
            clk_prepare_min = 38;
            clk_prepare_max = 96;
            clk_trail_min = 60;
            clk_zero_min = 200;
            eot_clk_max = 108 + 12 * ((1 / bitrate) * 1000);
            eot_data_max = 108 + 12 * ((1 / bitrate) * 1000);
            hs_prepare_min = 40 + 4 * ((1 / bitrate) * 1000);
            hs_prepare_max = 85 + 6 * ((1 / bitrate) * 1000);
            hs_zero_min = (145 + 10 * ((1 / bitrate) * 1000)) - (((hs_prepare_max + hs_prepare_min)) / 2);
            hs_trail_min = 60 + 4 * ((1 / bitrate) * 1000);
            hs_prepare_zero_min = 145 + 10 * ((1 / bitrate) * 1000);
            clk_prepare_zero_min = 300;
            ///typical timing 
            typ_hs_prepare = (hs_prepare_max + hs_prepare_min) / 2;
            typ_hs_zero = (hs_prepare_zero_min - hs_prepare_min) * 1.1F;
            typ_hs_exit = 120;
            typ_hs_trail = hs_trail_min * 1.2F;
            typ_clk_prepare = 70;
            typ_clk_zero = 240;
            typ_clk_trail = 80;
            typ_clk_pre = 10;
            typ_clk_post = clk_post_min * 1.2F;
            get_typcal_setting();
        }

        /// <summary>
        /// get 計算標準Actiming
        /// </summary>
        public void get_typcal_setting()
        {
            hs_prepare = typ_hs_prepare > p338_timing_step * 2 ? (float)Math.Ceiling((Math.Floor(typ_hs_prepare / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            hs_zero = typ_hs_zero > p338_timing_step ? (float)Math.Ceiling((Math.Floor(typ_hs_zero / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            hs_exit = typ_hs_exit > p338_timing_step ? (float)Math.Ceiling((Math.Floor(typ_hs_exit / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            hs_trail = typ_hs_trail > p338_timing_step ? (float)Math.Ceiling((Math.Floor(typ_hs_trail / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            clk_prepare = typ_clk_prepare > p338_timing_step ? (float)Math.Ceiling((Math.Floor(typ_clk_prepare / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            clk_zero = typ_clk_zero > p338_timing_step ? (float)Math.Ceiling((Math.Floor(typ_clk_zero / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            clk_trail = typ_clk_trail > p338_timing_step ? (float)Math.Ceiling((Math.Floor(typ_clk_trail / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            clk_pre = typ_clk_pre > p338_timing_step ? (float)Math.Ceiling((Math.Floor(typ_clk_pre / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            clk_post = typ_clk_post > p338_timing_step ? (float)Math.Ceiling((Math.Floor(typ_clk_post / p338_timing_step)) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
            TA_go = 266;
            T_wakeup = 1000000;
        }

        public void get_hsprepare_min_setting()
        {
            get_typcal_setting();
            hs_prepare = hs_prepare_min > p338_timing_step * 2 ? (float)Math.Ceiling((Math.Floor(hs_prepare_min / p338_timing_step) - 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }


        public void get_hsprepare_max_setting()
        {
            get_typcal_setting();
            hs_prepare = hs_prepare_max > p338_timing_step ? (float)Math.Ceiling((Math.Floor(hs_prepare_max / p338_timing_step) + 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_hsprepare_zero_min_setting() //hs prepare typ + hs zero min
        {
            get_typcal_setting();
            hs_zero = hs_zero_min > p338_timing_step * 2 ? (float)Math.Ceiling((Math.Floor(hs_zero_min / p338_timing_step) - 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_hs_trail_min_setting()
        {
            get_typcal_setting();
            hs_trail = hs_trail_min > p338_timing_step * 2 ? (float)Math.Ceiling((Math.Floor(hs_trail_min / p338_timing_step) - 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_hs_trail_max_setting()
        {
            get_typcal_setting();
            hs_trail = eot_data_max > p338_timing_step ? (float)Math.Ceiling((Math.Floor(eot_data_max / p338_timing_step) + 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_clk_trail_min_setting()
        {
            get_typcal_setting();
            clk_trail = clk_trail_min > p338_timing_step * 2 ? (float)Math.Ceiling((Math.Floor(clk_trail_min / p338_timing_step) - 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_clk_trail_max_setting()
        {
            clk_trail = eot_clk_max > p338_timing_step ? (float)Math.Ceiling((Math.Floor(eot_clk_max / p338_timing_step) + 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_clk_post_min_setting()
        {
            get_typcal_setting();
            clk_post = clk_post_min > p338_timing_step ? (float)Math.Ceiling((Math.Floor(clk_post_min / p338_timing_step) - 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_clk_pre_min_setting()
        {
            get_typcal_setting();
            clk_pre = clk_pre_min > p338_timing_step * 2 ? (float)Math.Ceiling((Math.Floor(clk_pre_min / p338_timing_step) - 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_clk_prepare_max_setting()//+
        {
            get_typcal_setting();
            clk_prepare = clk_prepare_max > p338_timing_step ? (float)Math.Ceiling((Math.Floor(clk_prepare_max / p338_timing_step) + 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_clk_prepare_min_setting()
        {
            get_typcal_setting();
            clk_prepare = clk_prepare_min > p338_timing_step * 2 ? (float)Math.Ceiling((Math.Floor(clk_prepare_min / p338_timing_step) - 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void get_clk_prepare_zero_min_setting()
        {
            get_typcal_setting();
            clk_prepare = clk_zero_min > p338_timing_step * 2 ? (float)Math.Ceiling((Math.Floor(clk_zero_min / p338_timing_step) - 1) * p338_timing_step) : (float)Math.Ceiling(p338_timing_step);
        }

        public void cal_video_bitrate(Auto_Test_Thread.MIPI_Parameter MIPI)
        {
            MIPI_Parameter.bitrate = ((MIPI_Parameter.vbp + MIPI_Parameter.vfp + MIPI_Parameter.vsa + MIPI_Parameter.vact) * (MIPI_Parameter.hbp + MIPI_Parameter.hfp + MIPI_Parameter.hsa + MIPI_Parameter.hact)
                * MIPI_Parameter.framerate * MIPI_Parameter.bbp) / MIPI_Parameter.lane;
        }

        public void Set_Test_Condition(Auto_Test_Thread.MIPI_Parameter MIPI_in)
        {
            MIPI_Parameter = MIPI_in;
        }

        public string get_timing_setting()
        {
            string reslut = null;
            reslut = "bitrate=" + bitrate + "\t" + "hs_prepare=" + hs_prepare + "\t" + "hs_zero=" + hs_zero + "\t" + "hs_exit=" + hs_exit + "\t" + "hs_trail = " + hs_trail + "\t" + "clk_prepare=" + clk_prepare + "\t" + "clk_zero=" + clk_zero + "\t" + "clk_trail=" + clk_trail + "\t" + "clk_pre=" + clk_pre + "\t" + "clk_post=" + clk_post + "\r\n";
            return reslut;
        }

    }
}
