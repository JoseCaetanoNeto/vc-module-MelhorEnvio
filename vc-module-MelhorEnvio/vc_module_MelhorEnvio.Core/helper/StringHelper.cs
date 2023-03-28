using System.Text.RegularExpressions;

namespace vc_module_MelhorEnvio.Core
{
    public static class StringHelper
    {
        static Regex s_regOnlyNr = new Regex(@"[^0-9]");
        static Regex s_regRemovCharEx = new Regex(@"[^a-z;A-Z;0-9;\s]");

        static public string OnlyN(this string pString)
        {
            if (string.IsNullOrEmpty(pString))
                return pString;

            return s_regOnlyNr.Replace(pString, string.Empty);
        }

        static public string OnlyAlfaNum(this string pString)
        {
            if (string.IsNullOrEmpty(pString))
                return pString;

            return s_regRemovCharEx.Replace(pString, string.Empty);
        }

        static public string Max(this string pString, int pLength)
        {
            if (string.IsNullOrEmpty(pString) || pString.Length <= pLength)
                return pString;

            return pString.Substring(0, pLength);
        }
    }
}
