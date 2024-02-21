using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MES
{
    interface MesInterface
    {
        string[] GetDllInfo();
        object Interface(Dictionary<string, object> keys);
        string Run(object[] obj);
    }
}
