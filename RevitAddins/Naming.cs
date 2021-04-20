using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RevitAddins
{
    class Naming
    {
        private static string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string NextString(string curString, int count)
        {
            string next;
            if (Regex.IsMatch(curString, @"^\d{1,2}$"))
            {
                int index = int.Parse(curString);
                next = (index + count).ToString();
            }
            else if (Regex.IsMatch(curString, @"^(\d{1,2}-\d{1,2}$|[A-Z]-\d{1,2})"))
            {
                int index = int.Parse(curString.Substring(curString.IndexOf("-") + 1));
                next = curString.Substring(0, curString.IndexOf("-")) + (index + count).ToString();
            }
            else if (Regex.IsMatch(curString, @"^[A-Z]$"))
            {
                int index = alphabet.IndexOf(curString);
                next = index + count < 26 ? alphabet[index + count].ToString() : String.Format("{0}-{1}", alphabet[(index + count) / 26 - 1], alphabet[(index + count) % 26]);
            }
            else if (Regex.IsMatch(curString, @"^(([A-Z])\2)$"))
            {
                int index = alphabet.IndexOf(curString[0]);
                next = index + count < 26 ? String.Format("{0}{0}", alphabet[index + count].ToString()) : String.Format("{0}-{1}", alphabet[(index + count) / 26 - 1], alphabet[(index + count) % 26]);
            }
            else
            {
                int index = alphabet.IndexOf(curString[0]) * 26 + alphabet.IndexOf(curString[2]);
                next = String.Format("{0}-{1}", alphabet[(index + count) / 26], alphabet[(index + count) % 26]);
            }
            return next;
        }
    }
}
