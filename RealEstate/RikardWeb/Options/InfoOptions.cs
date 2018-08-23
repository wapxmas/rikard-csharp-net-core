using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Options
{
    public class InfoOptions
    {
        public Phone Phone { get; set; }
        public Email Email { get; set; }
        public Host Host { get; set; }
        public string SmsNotifyPhone { get; set; }
        public Robokassa Robokassa { get; set; }
    }

    public class Robokassa
    {
        public string Pass1 { get; set; }
        public string Pass2 { get; set; }
        public string MerchantLogin { get; set; }
        public double HourPrice { get; set; }
        public double DayPrice { get; set; }
        public double WeekPrice { get; set; }
        public double MonthPrice { get; set; }
    }

    public class Host
    {
        public string Name { get; set; }
        public string Scheme { get; set; }
    }

    public class Email
    {
        public string SmtpAddress { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
    }

    public class Phone
    {
        public string Country { get; set; }
        public string Code { get; set; }
        public string Number { get; set; }

        public HtmlString PlainPhone()
        {
            return new HtmlString($"{Country}{new String(NoCountry().Where(Char.IsDigit).ToArray())}");
        }

        public HtmlString PhoneHtmlBoldNoCountry()
        {
            return new HtmlString($"{Code} {PhoneHtmlBoldPlain()}");
        }

        public HtmlString PhoneHtmlBold()
        {
            return new HtmlString($"{Country} {Code} {PhoneHtmlBoldPlain()}");
        }

        public HtmlString PhoneHtmlFontBold()
        {
            return new HtmlString($"{Country} {Code} <strong style=\"font - size: 16px; color: #000000;\">{Number}</strong>");
        }

        public HtmlString Complete()
        {
            return new HtmlString($"{Country} {NoCountry()}");
        }

        private string NoCountry()
        {
            return $"{Code} {Number}";
        }

        private string PhoneHtmlBoldPlain()
        {
            return $"<span style=\"font-weight: bold\">{Number}</span>";
        }
    }
}
