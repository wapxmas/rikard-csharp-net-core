using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.AspSmsRu.SmsRu.Options
{
    public class SmsRuOptions
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string ApiId { get; set; }
        public string PartnerId { get; set; }
        public string From { get; set; }
        public bool Translit { get; set; }
        public bool Test { get; set; }
    }
}
