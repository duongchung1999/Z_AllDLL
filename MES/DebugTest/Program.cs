using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BD;
using SwATE_Net;

namespace DebugTest
{
    class Program
    {
        static SwATE mes;
        static void Main(string[] args)
        {
            mes = new SwATE();
            mes.GetPart("");
            string test = mes.GetKeyPartInfo("2340ME00A4Q8", "DONGLE ASS’Y HDT647-010 BLK SAMR LASER V");
            // string aa = mes.checkEmpNo("91204127", "SY002_F_MIC_test 919" );
            //string SN = mes.checkSN_Station("2310ME00CSQ8", "T2.3TEST647");
            //string aa = mes.sendTestResult("91204127", "VNH32903DY", "HDT655 T2.7","NG;ST0004;");
            //string test = mes.getbd("2315ME00CET8", 11);
            //string aa = mes.GetMAC_SY("01023200013044");
            //string aa = mes.Query_Bluetooth_ID("2312ME00F388");
            //string aa = mes.InsertDCSN("2315ME00CET8", 11, "TX2314000542");
            //string bb = mes.Query_Link_SN("2315ME00CET8", 11);

            //string bd = mes.Query_Bluetooth_ID("VN000000000001C40034N/A");
        //    string aa = mes.checkEmpNo("91202346", "CorePack-15");
         //   string bb = mes.GetPart("002100T04");

            //Console.WriteLine(aa);
            //Console.WriteLine(bb);
            Console.ReadKey();
        }
    }
}
