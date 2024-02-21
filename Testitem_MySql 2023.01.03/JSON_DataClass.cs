using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace JSON_DATA
{
    /*
     {
  "TestPlan": "HDT624.dll",
  "BU": "VC Headset",
  "Project": "Mulberries",
  "Station": "T2.0",
  "Stage": "MP",
  "Items": {
    "UID": {
      "U_Limit": "",
      "L_Limit": "",
      "EID": "E0001",
      "Description": "UID"
    },
    "current Test": {
      "U_Limit": "40",
      "L_Limit": "20",
      "EID": "E0002",
      "Description": "current Test"
    }
  }
}
     */
    public class Account
    {
        public string TestPlan { get; set; }
        public string BU { get; set; }
        public string Project { get; set; }
        public string Station { get; set; }
        public string Stage { get; set; }
        public string oemSource { get; set; }


        public Dictionary<string, Dictionary<string, string>> Items { get; set; }
    }



    class JSON_DataClass
    {
        public static string TestTableToJSON(string TypeName, string BU, string Project, string LogiStation, string Stage, string oemSource, ListView TestTable)
        {
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();
            for (int i = 0; i < TestTable.Items.Count; i++)
            {
                string itemsName = $"{TestTable.Items[i].SubItems[1].Text.Replace(",", "")}";
                if (data.ContainsKey(itemsName))
                {
                }

                Dictionary<string, string> Limits = new Dictionary<string, string>()
                        {
                            {"U_Limit",TestTable.Items[i].SubItems[4].Text.Replace(",", "") },
                            {"L_Limit",TestTable.Items[i].SubItems[3].Text.Replace(",", "") },
                            {"EID",$"E{(i+1).ToString().PadLeft(3,'0')}" },
                             {"Description",itemsName }
                        };
                data.Add(itemsName, Limits);
            }
            Account ss = new Account()
            {
                TestPlan = TypeName,
                BU = BU,
                Project = Project,
                Station = LogiStation,
                Stage = Stage,
                oemSource = oemSource,
                Items = data
            };

            string json = JsonConvert.SerializeObject(ss);

            string _var = json.Trim();
            //判读是数组还是对象
            if (_var.StartsWith("["))
            {
                JArray jobj = JArray.Parse(json);
                json = jobj.ToString();
            }
            else if (_var.StartsWith("{"))
            {
                JObject jobj = JObject.Parse(json);
                json = jobj.ToString();
            }
            return json;

        }
    }
}
