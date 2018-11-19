using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;

namespace P338_Auto_Tool
{
    class MIPI_Auto_Test
    {
        _Application excel = new Excel.Application();
        Workbook wb;
        Worksheet ws;

        public void Excel_open(string path, int sheet)
        {
            wb = excel.Workbooks.Open(path);
            ws = wb.Worksheets[sheet];
        }

        public void Excel_sheet_select(int sheet)
        {
            ws = wb.Worksheets[sheet];
        }

        public void EXcel_sheet_select(string sheet)
        {
            ws = wb.Worksheets[sheet];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">Y方向</param>
        /// <param name="j">X方向</param>
        /// <returns></returns>
        public string Read_Excel_cell(int i, int j)
        {
            if (ws.Cells[i, j].Value2 != null)
            {
                return ws.Cells[i, j].Value2.ToString();
            }
            else
            {
                return "";
            }
        }

        public void Save_Excel()
        {
            wb.CheckCompatibility = false;
            wb.Save();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">Y方向</param>
        /// <param name="j">X方向</param>
        /// <param name="input"></param>
        public void Write_Excel_cell(int i, int j, string input)
        {
            ws.Cells[i, j].Value2 = input;
        }
        public List<string> Read_Excel_Column(int x , int y )
        {
            List<string> reslut = new List<string>();
            for(int i = y; i <= 65535 ; i++)
            {
                string temp;
                temp = Read_Excel_cell(i, x);
                if (temp == "") break;
                reslut.Add(temp);                
            }
            return reslut;

        }
        public void Close_Excel()
        {
            wb.Close();
        }
        /// <summary>
        /// <param> input a , input b</param>       
        /// </summary>                
        public int get_project(int a, int b)
        {
            return a + b;
        }

        /// <summary>
        /// this is function
        /// </summary>
        /// <param name="a">bigger number</param>
        /// <param name="b">smaller number</param>
        /// <returns>a-b</returns>
        /// <remarks>this is test function</remarks>
        public int new_project(int a, int b)
        {
            return a - b;
        }


        public List<List<int>> testlist(List<int> input)
        {
            List<List<int>> output = new List<List<int>>(3);
            int count = 1;
            while (count < input.Count)
            {
                int temp = input[count];
                output[0].Add(temp);
                count++;
                for (int i = 0; i < output[1][1]; i++)
                {
                    output[1].Add(input[count]);
                    count++;
                }
            }


            return output;
        }

        public List<List<List<int>>> testlist2(int[] input)
        {
            List<List<List<int>>> output = new List<List<List<int>>>();
            int count = 0;
            int datacount = 0;
            while (count < input.Length - 2)
            {
                if (input[count] == 01)///確定是否為一列read開頭
                {
                    count++;
                    output.Add(new List<List<int>>());
                    ///在這一列結果增加WC , Data , Err 三個list
                    output[datacount].Add(new List<int>());
                    output[datacount].Add(new List<int>());
                    output[datacount].Add(new List<int>());
                    ///將WC加入
                    ///若為Read
                    if (input[count] == 0x87)
                    {
                        count++;
                        if (input[count] == 0x02)
                        {
                            count++;
                            output[datacount][0].Add(0);
                            output[datacount][1].Add(0);
                            output[datacount][2].Add(input[count++]);
                            output[datacount][2].Add(input[count++]);
                            count++;
                        }
                        else if (input[count] == 0x1c)
                        {
                            count++;
                            output[datacount][0].Add(input[count]);//2wc + 1ecc
                            count += 3;
                            for (int i = 0; i < output[datacount][0][0]; i++)
                            {
                                output[datacount][1].Add(input[count++]);
                            }
                            count += 2;
                        }
                        else if (input[count] == 0x21)
                        {
                            count++;
                            output[datacount][0].Add(1);
                            output[datacount][1].Add(input[count++]);
                            count += 2;
                        }
                        else if (input[count] == 0x22)
                        {
                            count++;
                            output[datacount][0].Add(2);
                            output[datacount][1].Add(input[count++]);
                            output[datacount][1].Add(input[count++]);
                            count++;
                        }

                    }
                    else if (input[count] == 0x84)///ack
                    {
                        output[datacount][0].Add(0);
                        output[datacount][1].Add(0);
                        output[datacount][2].Add(0);
                        count++;
                    }
                }
                if (count == input.Length)
                    return output;
                if (input[count] == 02)
                {
                    count++;
                    output[datacount][2].Add(input[count]);
                    count++;
                    output[datacount][2].Add(input[count]);
                    count += 2;
                }

                datacount++;


            }
            return output;

        }


    }
}

