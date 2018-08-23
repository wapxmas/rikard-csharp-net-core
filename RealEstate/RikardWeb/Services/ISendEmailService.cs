using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RikardWeb.Data;

namespace RikardWeb.Services
{
    public interface ISendEmailService
    {
        void Send(EmailLetter letter);
    }
}
