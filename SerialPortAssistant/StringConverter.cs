using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortAssistant
{
    class StringConverter
    {
        public static string HexStringToString(string hexString)
        {
            if (hexString.Length % 3 != 1)
            {
                byte[] bytes = HexStringToByteArray(hexString);
                string originString = Encoding.Default.GetString(bytes);
                return originString;
            }
            return hexString;
        }

        public static string StringToHexString(string origin)
        {
            byte[] bytes = Encoding.Default.GetBytes(origin);
            string hexString = BitConverter.ToString(bytes).Replace("-", " ");
            return hexString;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 3 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[][] HexStringsToByteArrays(string[] hexs)
        {
            return Enumerable.Range(0, hexs.Length)
                             .Select(x => HexStringToByteArray(hexs[x]))
                             .ToArray();
        }
    }
}
