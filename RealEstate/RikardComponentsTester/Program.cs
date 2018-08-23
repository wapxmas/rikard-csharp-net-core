using System;
using System.Linq;
using RikardWeb.Lib.Db;
using RikardWeb.Lib.News;
using System.Threading.Tasks;
using RikardLib.Text;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace RikardComponentsTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Task.Run(async () => await MainAsync(args)).GetAwaiter().GetResult();
            //Console.WriteLine(DateTime.Now.ToString());
            //Console.WriteLine(DateTime.Now.ToUniversalTime().ToString());
            //Console.WriteLine((Int32)((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds);
            //string md5test = "//Console.WriteLine(DateTime.Now.ToUniversalTime().ToString());";
            //Console.WriteLine(MD5Hash(md5test));
            double dOutSum;
            double.TryParse("1000.4577", NumberStyles.Number, CultureInfo.InvariantCulture, out dOutSum);
            Console.WriteLine(dOutSum.ToString(CultureInfo.InvariantCulture));
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

        //private static async Task MainAsync(string[] args)
        //{
        //    var creds = new MongoDbCredentials("rikard", "123456", "rikardru", "127.0.0.1");

        //    await NewsTweeter.RunNewsTweeter(creds);
        //}
    }
}