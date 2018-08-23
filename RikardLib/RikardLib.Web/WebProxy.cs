using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RikardLib.Web
{
    public class WebProxy : IWebProxy
    {
        public ICredentials Credentials { get; set; }

        public Uri Uri { get; private set; }

        public WebProxy(string uri)
        {
            this.Uri = new Uri(uri);
        }

        public WebProxy(string host, int port) : 
            this($"http://{host}:{port}")
        {
            
        }

        public Uri GetProxy(Uri destination)
        {
            throw new NotImplementedException();
        }

        public bool IsBypassed(Uri host)
        {
            throw new NotImplementedException();
        }
    }
}
