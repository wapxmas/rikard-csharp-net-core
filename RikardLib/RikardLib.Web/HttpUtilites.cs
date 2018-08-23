using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RikardLib.Web
{
    public static class HttpUtilites
    {
        public static async Task<(byte[] Data, string Filename, HttpStatusCode Status, string ContentType)> DownloadFileToMem(string requestUri, int timeoutSec = 30)
        {
            var filename = string.Empty;
            var contentType = string.Empty;

            try
            {
                using (HttpClientHandler rqHandler = new HttpClientHandler())
                {
                    rqHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    rqHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    rqHandler.UseProxy = false;
                    rqHandler.AllowAutoRedirect = true;

                    using (var client = new HttpClient(rqHandler))
                    {
                        if (timeoutSec > 0)
                        {
                            client.Timeout = TimeSpan.FromSeconds(timeoutSec);
                        }

                        Console.WriteLine(requestUri);

                        using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                        {
                            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:53.0) Gecko/20100101 Firefox/53.0");

                            using (HttpResponseMessage response = await client.SendAsync(request))
                            {
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    if(response.Content.Headers.TryGetValues("Content-Disposition", out IEnumerable<string> vs))
                                    {
                                        var cd = vs.FirstOrDefault();

                                        if(!string.IsNullOrWhiteSpace(cd))
                                        {
                                            var lookFor1 = "filename*=UTF-8''";
                                            var lookFor2 = "filename=";

                                            if (cd.IndexOf(lookFor1, StringComparison.Ordinal) != -1)
                                            {
                                                var fnField = cd.Split(';').Single(v => v.Contains(lookFor1));
                                                filename = fnField.Substring(fnField.IndexOf(lookFor1,
                                                    StringComparison.CurrentCultureIgnoreCase) + lookFor1.Length);
                                                filename = WebUtility.UrlDecode(filename);
                                            }
                                            else if (cd.IndexOf(lookFor2, StringComparison.Ordinal) != -1)
                                            {
                                                var fnField = cd.Split(';').Single(v => v.Contains(lookFor2));
                                                filename = fnField.Substring(fnField.IndexOf(lookFor2,
                                                    StringComparison.CurrentCultureIgnoreCase) + lookFor2.Length);
                                                filename = WebUtility.UrlDecode(filename);
                                            }
                                        }
                                    }

                                    if (response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> ctvs))
                                    {
                                        contentType = ctvs.FirstOrDefault();
                                    }
                                }

                                return (await response.Content.ReadAsByteArrayAsync(), filename, response.StatusCode, contentType);
                            }
                        }
                    }
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.Timeout)
            {
                return (new byte[] { }, filename, HttpStatusCode.RequestTimeout, contentType);
            }
            catch (TaskCanceledException)
            {
                return (new byte[] { }, filename, HttpStatusCode.RequestTimeout, contentType);
            }
        }

        public static async Task<HttpStatusCode> DownloadAsync(string requestUri, string filename, int timeoutSec = 30)
        {
            try
            {
                using (HttpClientHandler rqHandler = new HttpClientHandler())
                {
                    rqHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    rqHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    rqHandler.UseProxy = false;

                    using (var client = new HttpClient(rqHandler))
                    {
                        if(timeoutSec > 0)
                        {
                            client.Timeout = TimeSpan.FromSeconds(timeoutSec);
                        }

                        using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                        using (HttpResponseMessage response = await client.SendAsync(request))
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        using (Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                        {
                            await contentStream.CopyToAsync(stream);

                            return response.StatusCode;
                        }
                    }
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.Timeout)
            {
                return HttpStatusCode.RequestTimeout;
            }
            catch(TaskCanceledException)
            {
                return HttpStatusCode.RequestTimeout;
            }
        }

        public static async Task<byte[]> GetBytesByUrl(string url, int timeoutSec = 5, bool allowAutoRedirect = false)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            using (HttpClientHandler rqHandler = new HttpClientHandler())
            {
                rqHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                rqHandler.AllowAutoRedirect = allowAutoRedirect;
                rqHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                rqHandler.UseProxy = false;
                using(HttpClient rqClient = new HttpClient(rqHandler))
                {
                    if (timeoutSec > 0)
                    {
                        rqClient.Timeout = TimeSpan.FromSeconds(timeoutSec);
                    }

                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
                    using (HttpResponseMessage response = await rqClient.SendAsync(request))
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                }
            }
        }

        public static async Task<string> GetStringByUrl(string url, int timeoutSec = 5, bool allowAutoRedirect = false)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            using (HttpClientHandler rqHandler = new HttpClientHandler())
            {
                rqHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                rqHandler.AllowAutoRedirect = allowAutoRedirect;
                rqHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                rqHandler.UseProxy = false;
                using (HttpClient rqClient = new HttpClient(rqHandler))
                {
                    if (timeoutSec > 0)
                    {
                        rqClient.Timeout = TimeSpan.FromSeconds(timeoutSec);
                    }

                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
                    using (HttpResponseMessage response = await rqClient.SendAsync(request))
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }

        public static async Task<bool> IsUrlExists(string url, int timeoutSec = 5)
        {
            if(string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            using(HttpClientHandler rqHandler = new HttpClientHandler())
            {
                rqHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                rqHandler.AllowAutoRedirect = false;
                rqHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                rqHandler.UseProxy = false;
                using (HttpClient rqClient = new HttpClient(rqHandler))
                {
                    if (timeoutSec > 0)
                    {
                        rqClient.Timeout = TimeSpan.FromSeconds(timeoutSec);
                    }
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url))
                    using (HttpResponseMessage response = await rqClient.SendAsync(request))
                    {
                        return response.StatusCode == HttpStatusCode.OK;
                    }
                }
            }
        }
    }
}
