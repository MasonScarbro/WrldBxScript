using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public static class StringHelpers
    {
        public static string ConvertBoolString(bool value) => value.ToString().ToLower();

        public static string ToParaCase(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
        }
        public static string ReplaceWhiteSpace(string str)
        {
            try
            {
                str.Replace(' ', '_');
            }
            catch (Exception)
            {

            }

            return str;
        }

        public static string InQoutes(string str)
        {
            return '"' + str + '"';
        }
    }
}
