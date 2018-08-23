using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Globalization;
using System.Threading.Tasks;

namespace RikardLib.Web
{
    public class HttpRequestResult
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Data { get; set; }
    }

    public static class HttpRequest
    {
        private const int DATE_TOKEN_GMT = -1000;

        private const int DATE_TOKEN_LAST = DATE_TOKEN_GMT;

        private const int DATE_TOKEN_ERROR = (DATE_TOKEN_LAST + 1);

        private const int DATE_TOKEN_JANUARY = 1;
        private const int DATE_TOKEN_FEBRUARY = 2;
        private const int DATE_TOKEN_MARCH = 3;
        private const int DATE_TOKEN_APRIL = 4;
        private const int DATE_TOKEN_MAY = 5;
        private const int DATE_TOKEN_JUNE = 6;
        private const int DATE_TOKEN_JULY = 7;
        private const int DATE_TOKEN_AUGUST = 8;
        private const int DATE_TOKEN_SEPTEMBER = 9;
        private const int DATE_TOKEN_OCTOBER = 10;
        private const int DATE_TOKEN_NOVEMBER = 11;
        private const int DATE_TOKEN_DECEMBER = 12;

        private const int DATE_TOKEN_SUNDAY = 0;
        private const int DATE_TOKEN_MONDAY = 1;
        private const int DATE_TOKEN_TUESDAY = 2;
        private const int DATE_TOKEN_WEDNESDAY = 3;
        private const int DATE_TOKEN_THURSDAY = 4;
        private const int DATE_TOKEN_FRIDAY = 5;
        private const int DATE_TOKEN_SATURDAY = 6;

        public static async Task<HttpRequestResult> HttpGetRequest(string uri, string ua, NameValueCollection values = null, bool allowRedirect = false,
            CookieCollection cookies = null, int automaticRedirections = 0, string redirectDomainEndsWith = null, string httpProxy = null, int timeouts = 0)
        {
            if (values != null && values.Count > 0)
            {
                List<string> queryValues = new List<string>();

                foreach (KeyValuePair<string, string> v in values.AllKeys.SelectMany(values.GetValues, (k, v) => new KeyValuePair<string, string>(k, v)))
                {
                    queryValues.Add(string.Format("{0}={1}", v.Key, WebUtility.UrlEncode(v.Value)));
                }

                string queryString = string.Join("&", queryValues.ToArray());

                uri = string.Format("{0}?{1}", uri, queryString);
            }

            using(HttpClientHandler rqHandler = new HttpClientHandler())
            {
                rqHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                rqHandler.AllowAutoRedirect = false;
                rqHandler.MaxAutomaticRedirections = 10;
                rqHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                if (httpProxy != null)
                {
                    string[] httpProxyA = httpProxy.Split(':');

                    if (httpProxyA.Length == 2)
                    {
                        int port;

                        if (int.TryParse(httpProxyA[1], out port))
                        {
                            rqHandler.Proxy = new WebProxy(httpProxyA[0], port);
                            rqHandler.UseProxy = true;
                        }
                    }
                    else
                    {
                        rqHandler.Proxy = new WebProxy(string.Format("http://{0}", httpProxy));
                        rqHandler.UseProxy = true;
                    }
                }
                else
                {
                    rqHandler.UseProxy = false;
                }

                using (HttpClient rqClient = new HttpClient(rqHandler))
                {
                    rqClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                    rqClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                    rqClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
                    rqClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
                    if (!string.IsNullOrEmpty(ua))
                    {
                        rqClient.DefaultRequestHeaders.Add("User-Agent", ua);
                    }
                    if (timeouts > 0)
                    {
                        rqClient.Timeout = TimeSpan.FromSeconds(timeouts);
                    }

                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        if (cookies != null)
                        {
                            SetCookiesToRequest(request, cookies);
                        }

                        using (HttpResponseMessage response = await rqClient.SendAsync(request))
                        {
                            if (cookies != null)
                            {
                                if (response.Headers.Contains("Set-Cookie"))
                                {
                                    StoreCookiesToCollection(response.Headers.GetValues("Set-Cookie").FirstOrDefault(),
                                        cookies, response.Content.Headers.ContentLocation.Host);
                                }
                            }

                            HttpStatusCode status = response.StatusCode;

                            bool isRedirect = status == HttpStatusCode.Redirect || status == HttpStatusCode.MovedPermanently;

                            if (allowRedirect && isRedirect && automaticRedirections < rqHandler.MaxAutomaticRedirections && response.Headers.Location != null)
                            {

                                bool isRedirectDomainEndsWith = true;

                                if (!string.IsNullOrEmpty(redirectDomainEndsWith))
                                {
                                    try
                                    {
                                        if (!response.Headers.Location.Host.ToLower().EndsWith(redirectDomainEndsWith.ToLower()))
                                        {
                                            isRedirectDomainEndsWith = false;
                                        }
                                    }
                                    catch (Exception) { }
                                }

                                if (isRedirectDomainEndsWith)
                                {
                                    return await HttpGetRequest(response.Headers.Location.ToString(), ua, null, allowRedirect, cookies, automaticRedirections + 1, redirectDomainEndsWith, httpProxy, timeouts);
                                }
                            }

                            using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                            using (StreamReader readStream = new StreamReader(responseStream, GetEncodingFromCharSet(response.Content.Headers.ContentEncoding.FirstOrDefault())))
                            {
                                return new HttpRequestResult { HttpStatusCode = status, Data = await readStream.ReadToEndAsync() };
                            }
                        }
                    }
                }
            }
        }

        private static Encoding GetEncodingFromCharSet(string charSet)
        {
            Encoding encoding = Encoding.UTF8;

            if (!string.IsNullOrEmpty(charSet))
            {
                charSet = charSet.Replace("\"", "");
                charSet = charSet.Replace("'", "");

                switch (charSet.ToLower())
                {
                    case "cp-1251":
                    case "cp1251":
                        encoding = Encoding.GetEncoding(1251);
                        break;
                    case "none":
                        encoding = Encoding.UTF8;
                        break;
                    default:
                        encoding = Encoding.GetEncoding(charSet);
                        break;
                }
            }

            return encoding;
        }

        private static void SetCookiesToRequest(HttpRequestMessage request, CookieCollection cookies)
        {
            List<string> cookiesList = new List<string>();

            foreach (Cookie cook in cookies)
            {
                if (!cook.Expired)
                {
                    cookiesList.Add(cook.ToString());
                }
            }

            if (cookiesList.Count > 0)
            {
                request.Headers.Add("Cookie", string.Join("; ", cookiesList));
            }
        }

        public static void StoreCookiesToCollection(string rawCookies, CookieCollection cookiesCollection, string host)
        {
            List<string> cookies = new List<string>();

            if (string.IsNullOrEmpty(rawCookies))
            {
                return;
            }

            rawCookies = rawCookies.Replace("\r", "");
            rawCookies = rawCookies.Replace("\n", "");
            rawCookies = rawCookies.Trim();

            string[] strCookTemp = rawCookies.Split(',');

            int i = 0;

            int n = strCookTemp.Length;

            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    string newCookie = strCookTemp[i] + "," + strCookTemp[i + 1];

                    cookies.Add(newCookie.Trim());

                    i = i + 1;
                }
                else
                {
                    cookies.Add(strCookTemp[i].Trim());
                }

                i = i + 1;
            }

            if (cookies.Count == 0)
            {
                return;
            }

            int ccount = cookies.Count;

            string strEachCook;

            string[] strEachCookParts;

            for (i = 0; i < ccount; i++)
            {
                strEachCook = cookies[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;
                string strCNameAndCValue = string.Empty;
                string strPNameAndPValue = string.Empty;
                string strDNameAndDValue = string.Empty;
                string[] NameValuePairTemp;
                Cookie cookTemp = new Cookie();

                for (int j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];

                        if (!string.IsNullOrEmpty(strCNameAndCValue))
                        {
                            int firstEqual = strCNameAndCValue.IndexOf("=");

                            if (firstEqual >= 0)
                            {
                                string firstName = strCNameAndCValue.Substring(0, firstEqual);
                                string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                                cookTemp.Name = firstName;
                                cookTemp.Value = allValue;
                            }
                        }

                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        NameValuePairTemp = strEachCookParts[j].Split('=');

                        if (NameValuePairTemp.Length == 2)
                        {
                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                cookTemp.Path = NameValuePairTemp[1];
                            }
                            else
                            {
                                cookTemp.Path = "/";
                            }
                        }

                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        NameValuePairTemp = strEachCookParts[j].Split('=');

                        if (NameValuePairTemp.Length == 2)
                        {
                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                cookTemp.Domain = NameValuePairTemp[1];
                            }
                            else
                            {
                                cookTemp.Domain = host;
                            }
                        }

                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("expires", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        NameValuePairTemp = strEachCookParts[j].Split('=');

                        if (NameValuePairTemp.Length == 2)
                        {
                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                DateTime cookiesExpiresDate;

                                if (ParseCookieDate(NameValuePairTemp[1], out cookiesExpiresDate))
                                {
                                    cookTemp.Expires = cookiesExpiresDate;
                                }
                            }
                        }

                        continue;
                    }
                }

                if (cookTemp.Path == string.Empty)
                {
                    cookTemp.Path = "/";
                }
                if (cookTemp.Domain == string.Empty)
                {
                    cookTemp.Domain = host;
                }

                cookiesCollection.Add(cookTemp);
            }
        }

        public static bool ParseCookieDate(string dateString, out DateTime dtOut)
        {
            //
            // The format variants
            //
            // 1) .NET HttpCookie   = "dd-MMM-yyyy HH:mm:ss GMT'"
            // 2) Version0          = "dd-MMM-yy HH:mm:ss GMT"
            // 3) Some funky form   = "dd MMM yyyy HH:mm:ss GMT"
            //
            // In all above cases we also accept single digit dd,hh,mm,ss
            // That's said what IE does.

            if (dateString.IndexOf(",") >= 0)
            {
                dateString = dateString.Split(',')[1].Trim();
            }

            dtOut = DateTime.MinValue;
            char[] buffer = dateString.ToCharArray();
            char ch;

            if (buffer.Length < 18)
            { //cover all before "ss" in the longest case
                return false;
            }

            int idx = 0;
            // Take the date
            int day = 0;
            if (!Char.IsDigit(ch = buffer[idx++])) { return false; }
            else { day = ch - '0'; }
            if (!Char.IsDigit(ch = buffer[idx++])) { --idx; }                //one digit was used for a date
            else { day = day * 10 + (ch - '0'); }


            if (day > 31) { return false; }

            ++idx;  //ignore delimiter and position on Month

            // Take the Month
            int month = MapDayMonthToDword(buffer, idx);

            if (month == DATE_TOKEN_ERROR) { return false; }

            idx += 4; //position after Month and ignore delimiter

            // Take the year
            int year = 0;
            int i;
            for (i = 0; i < 4; ++i)
            {
                if (!Char.IsDigit(ch = buffer[i + idx]))
                {
                    // YY case
                    if (i != 2) { return false; }
                    else { break; }
                }
                year = year * 10 + (ch - '0');
            }

            //check for two digits
            if (i == 2)
            {
                year += ((year < 80) ? 2000 : 1900);
            }

            i += idx;       //from now on 'i' is used as an index

            if (buffer[i++] != ' ') { return false; }

            //Take the hour
            int hour = 0;
            if (!Char.IsDigit(ch = buffer[i++])) { return false; }
            else { hour = ch - '0'; }
            if (!Char.IsDigit(ch = buffer[i++])) { --i; }                     //accept single digit
            else { hour = hour * 10 + (ch - '0'); }

            if (hour > 24 || buffer[i++] != ':') { return false; }

            //Take the min
            int min = 0;
            if (!Char.IsDigit(ch = buffer[i++])) { return false; }
            else { min = ch - '0'; }
            if (!Char.IsDigit(ch = buffer[i++])) { --i; }                     //accept single digit
            else { min = min * 10 + (ch - '0'); }

            if (min > 60 || buffer[i++] != ':') { return false; }

            //Check that the rest will fit the buffer size "[s]s GMT"
            if ((buffer.Length - i) < 5) { return false; }

            //Take the sec
            int sec = 0;
            if (!Char.IsDigit(ch = buffer[i++])) { return false; }
            else { sec = ch - '0'; }
            if (!Char.IsDigit(ch = buffer[i++])) { --i; }                     //accept single digit
            else { sec = sec * 10 + (ch - '0'); }

            if (sec > 60 || buffer[i++] != ' ') { return false; }

            //Test GMT
            if ((buffer.Length - i) < 3 || buffer[i++] != 'G' || buffer[i++] != 'M' || buffer[i++] != 'T')
            {
                return false;
            }

            dtOut = new DateTime(year, month, day, hour, min, sec, 0).ToLocalTime();

            return true;
        }

        private static int MapDayMonthToDword(char[] lpszDay, int index)
        {
            switch (MAKE_UPPER(lpszDay[index]))
            { // make uppercase
                case 'A':
                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'P':
                            return DATE_TOKEN_APRIL;
                        case 'U':
                            return DATE_TOKEN_AUGUST;

                    }
                    return DATE_TOKEN_ERROR;

                case 'D':
                    return DATE_TOKEN_DECEMBER;

                case 'F':
                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'R':
                            return DATE_TOKEN_FRIDAY;
                        case 'E':
                            return DATE_TOKEN_FEBRUARY;
                    }

                    return DATE_TOKEN_ERROR;

                case 'G':
                    return DATE_TOKEN_GMT;

                case 'M':

                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'O':
                            return DATE_TOKEN_MONDAY;
                        case 'A':
                            switch (MAKE_UPPER(lpszDay[index + 2]))
                            {
                                case 'R':
                                    return DATE_TOKEN_MARCH;
                                case 'Y':
                                    return DATE_TOKEN_MAY;
                            }

                            // fall through to error
                            break;
                    }

                    return DATE_TOKEN_ERROR;

                case 'N':
                    return DATE_TOKEN_NOVEMBER;

                case 'J':

                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'A':
                            return DATE_TOKEN_JANUARY;

                        case 'U':
                            switch (MAKE_UPPER(lpszDay[index + 2]))
                            {
                                case 'N':
                                    return DATE_TOKEN_JUNE;
                                case 'L':
                                    return DATE_TOKEN_JULY;
                            }

                            // fall through to error
                            break;
                    }

                    return DATE_TOKEN_ERROR;

                case 'O':
                    return DATE_TOKEN_OCTOBER;

                case 'S':

                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'A':
                            return DATE_TOKEN_SATURDAY;
                        case 'U':
                            return DATE_TOKEN_SUNDAY;
                        case 'E':
                            return DATE_TOKEN_SEPTEMBER;
                    }

                    return DATE_TOKEN_ERROR;


                case 'T':
                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'U':
                            return DATE_TOKEN_TUESDAY;
                        case 'H':
                            return DATE_TOKEN_THURSDAY;
                    }

                    return DATE_TOKEN_ERROR;

                case 'U':
                    return DATE_TOKEN_GMT;

                case 'W':
                    return DATE_TOKEN_WEDNESDAY;

            }

            return DATE_TOKEN_ERROR;
        }

        private static char MAKE_UPPER(char c)
        {
            return (Char.ToUpperInvariant(c));
        }
    }
}
