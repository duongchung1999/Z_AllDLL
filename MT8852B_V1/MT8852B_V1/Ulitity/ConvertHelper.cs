using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtil
{
   public class ConvertHelper
    {
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bFormat">如果为True就加入空格</param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] data, bool bFormat = true)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            }
            string strResult = sb.ToString().Trim().ToUpper();
            if (!bFormat)
            {
                strResult = strResult.Replace(" ", "");
            }

            return strResult;
        }

        /// <summary>
        /// 16进制字符串转字节数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            return buffer;
        }      
    }
}
