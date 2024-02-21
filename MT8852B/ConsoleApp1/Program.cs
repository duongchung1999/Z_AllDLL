using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VISAInstrument.Port;

namespace ConsoleApp1
{
    internal class Program
    {
        static PortOperatorBase _portOperatorBase = null;
        static void Main(string[] args)
        {
            try
            {
                #region MyRegion
                _portOperatorBase = new GPIBPortOperator("GPIB0::27::INSTR");
                _portOperatorBase.Timeout = 50;
                _portOperatorBase.Open();
                _portOperatorBase.Clear();
                lvi_Visa.VISA visa = new lvi_Visa.VISA();
                visa._portOperatorBase = _portOperatorBase;
                List<string> liststr = new List<string>();
                for (int i = 0; i < 5000; i++)
                {

                    string str = visa.ReadFreqoffCW("39", i == 0);
                    Console.WriteLine($"读取的频偏{str}");

                    if (!double.TryParse(str, out double inx))
                    {
                        int cccc = 0;
                        liststr.Add(str);
                    };
                    Console.WriteLine($"读取的频偏{inx / 1000}");
                    Thread.Sleep(30);
                    //string str = Console.ReadLine();
                    //if (str.ToUpper() == "EX") break;
                    //_portOperatorBase.Write(str);
                    //Thread.Sleep(200);
                    //try
                    //{
                    //    Console.WriteLine(_portOperatorBase.Read());
                    //}
                    //catch
                    //{

                    //}

                }
                Console.ReadKey();
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }






        }


    }
}
