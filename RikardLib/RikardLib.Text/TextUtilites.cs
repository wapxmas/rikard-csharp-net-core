using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RikardLib.Text
{
    public static class TextUtilites
    {
        private static Random rng = new Random();

        private static string[] months =
        {
            "января",
            "февраля",
            "марта",
            "апреля",
            "мая",
            "июня",
            "июля",
            "августа",
            "сентября",
            "октября",
            "ноября",
            "декабря",
        };

        private static NumberFormatInfo nfiNumbersSpace = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

        static TextUtilites()
        {
            nfiNumbersSpace.NumberGroupSeparator = " ";
            nfiNumbersSpace.NumberDecimalSeparator = ",";
        }

        public static string HumanizeNumber(double number)
        {
            return number.ToString("#,0.00", nfiNumbersSpace);
        }

        public static string GetDeclension(int number, string nominativ, string genetiv, string plural)
        {
            number = number % 100;

            if (number >= 11 && number <= 19)
            {
                return plural;
            }

            switch (number % 10)
            {
                case 1:
                    return nominativ;
                case 2:
                case 3:
                case 4:
                    return genetiv;
                default:
                    return plural;
            }

        }

        public static string ToUpperFirstLetter(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            char[] letters = source.ToCharArray();

            letters[0] = char.ToUpper(letters[0]);

            return new string(letters);
        }

        public static string RemoveBetween(this string s, char begin, char end)
        {
            Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            return regex.Replace(s, string.Empty);
        }

        public static List<int> AllIndexesOf(this string str, string value)
        {
            List<int> indexes = new List<int>();

            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);

                if (index == -1)
                {
                    return indexes;
                }

                indexes.Add(index);
            }
        }

        public static string RemoveAround(this string input, string text)
        {
            return input.RemoveFromStart(text).RemoveFromEnd(text);
        }

        public static string RemoveFromStart(this string input, string text)
        {
            if(!input.StartsWith(text))
            {
                return input;
            }

            return input.Remove(0, text.Length);
        }

        public static string RemoveFromEnd(this string input, string text)
        {
            if (!input.EndsWith(text))
            {
                return input;
            }

            return input.Remove(input.Length - text.Length, text.Length);
        }

        public static string HideCRLF(this string input)
        {
            return input.Map(HideCRLFReplace);

            char HideCRLFReplace(char ch)
            {
                switch(ch)
                {
                    case '\r':
                        return ' ';
                    case '\n':
                        return ' ';
                    default:
                        return ch;
                }
            }
        }

        public static (string Value, int Index) IndexOfAny(this string str, string[] vals)
        {
            foreach(var v in vals)
            {
                int index = str.IndexOf(v);

                if (index >= 0)
                {
                    return (v, index);
                }
            }

            return (string.Empty, -1);
        }

        public static (string Word, int Index) IndexOfAnyWord(this string str, string[] words)
        {
            foreach (var w in words)
            {
                int index = str.IndexOf(w);

                if (index >= 0)
                {
                    if(index > 0 && char.IsLetter(str.ElementAt(index - 1)))
                    {
                        continue;
                    }

                    int aIndex = index + w.Length + 1;

                    if (char.IsLetter(w.Last()) && aIndex < str.Length && char.IsLetter(str.ElementAt(aIndex)))
                    {
                        continue;
                    }

                    return (w, index);
                }
            }

            return (string.Empty, -1);
        }

        public static string Map(this string input, Func<char, char> replace)
        {
            return new string(input.Select(replace).ToArray());
        }

        public static string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in result)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
                return sb.ToString();
            }
        }

        public static string DateTimeToHuman(DateTime date)
        {
            return $"{date.Day} {MonthToString(date.Month)} {date.Year} г.";
        }

        public static string MonthToString(int month)
        {
            if(month > 12 || month < 1)
            {
                return "undefuned";
            }
            return months[month - 1];
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IEnumerable<string> MakeUnique(this IEnumerable<string> list)
        {
            return new HashSet<string>(list).ToArray();
        }

        public static IEnumerable<string> SplitDefault(this String s)
            => s.SplitNoEmpty(null);

        public static IEnumerable<string> SplitNoEmpty(this String s, char[] separator)
            => s.Split(separator, StringSplitOptions.RemoveEmptyEntries)
            .Where(_ => !string.IsNullOrWhiteSpace(_));

        public static IEnumerable<string> SplitNoEmptyTrim(this String s, char[] separator)
            => s.Split(separator, StringSplitOptions.RemoveEmptyEntries)
            .Where(_ => !string.IsNullOrWhiteSpace(_))
            .Select(_ => _.Trim());

        public static String RemoveLatinLetters(this String s)
        {
            StringBuilder sb = new StringBuilder(s);

            RemoveLatinLetters(sb);

            return sb.ToString();
        }

        private static void RemoveLatinLetters(StringBuilder sb)
        {
            int dest = 0;

            bool previousIsLatin = false;

            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i].IsLatinLetter())
                {
                    if (!previousIsLatin)
                    {
                        previousIsLatin = true;
                        sb[dest] = ' ';
                        dest++;
                    }
                }
                else
                {
                    previousIsLatin = false;
                    sb[dest] = sb[i];
                    dest++;
                }
            }

            sb.Length = dest;
        }

        public static bool IsLatinLetter(this char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        public static String SaveLettersAndNumbers(this String s)
        {
            StringBuilder sb = new StringBuilder(s);

            SaveLettersAndNumbers(sb);

            return sb.ToString();
        }

        private static void SaveLettersAndNumbers(StringBuilder sb)
        {
            int dest = 0;

            bool previousIsNotChar = false;

            for (int i = 0; i < sb.Length; i++)
            {
                if (!Char.IsLetterOrDigit(sb[i]))
                {
                    if (!previousIsNotChar)
                    {
                        previousIsNotChar = true;
                        sb[dest] = ' ';
                        dest++;
                    }
                }
                else
                {
                    previousIsNotChar = false;
                    sb[dest] = sb[i];
                    dest++;
                }
            }

            sb.Length = dest;
        }

        public static String SaveOnlyLetters(this String s)
        {
            StringBuilder sb = new StringBuilder(s);

            SaveOnlyLetters(sb);

            return sb.ToString();
        }

        private static void SaveOnlyLetters(StringBuilder sb)
        {
            int dest = 0;

            bool previousIsNotChar = false;

            for (int i = 0; i < sb.Length; i++)
            {
                if (!Char.IsLetter(sb[i]))
                {
                    if (!previousIsNotChar)
                    {
                        previousIsNotChar = true;
                        sb[dest] = ' ';
                        dest++;
                    }
                }
                else
                {
                    previousIsNotChar = false;
                    sb[dest] = sb[i];
                    dest++;
                }
            }

            sb.Length = dest;
        }

        public static String TrimAndCompactWhitespaces(this String s)
        {
            StringBuilder sb = new StringBuilder(s);

            TrimAndCompactWhitespaces(sb);

            return sb.ToString();
        }

        private static void TrimAndCompactWhitespaces(StringBuilder sb)
        {
            if (sb.Length == 0)
                return;

            int start = 0;

            while (start < sb.Length)
            {
                if (Char.IsWhiteSpace(sb[start]))
                    start++;
                else
                    break;
            }

            if (start == sb.Length)
            {
                sb.Length = 0;
                return;
            }

            int end = sb.Length - 1;

            while (end >= 0)
            {
                if (Char.IsWhiteSpace(sb[end]))
                    end--;
                else
                    break;
            }

            int dest = 0;

            bool previousIsWhitespace = false;

            for (int i = start; i <= end; i++)
            {
                if (Char.IsWhiteSpace(sb[i]))
                {
                    if (!previousIsWhitespace)
                    {
                        previousIsWhitespace = true;
                        sb[dest] = ' ';
                        dest++;
                    }
                }
                else
                {
                    previousIsWhitespace = false;
                    sb[dest] = sb[i];
                    dest++;
                }
            }

            sb.Length = dest;
        }
    }
}
