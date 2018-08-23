using SmsRu.Enumerations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmsRu
{
    public interface ISmsRuService
    {
        /// <summary>
        /// Совершает отправку СМС сообщения одному получателю.
        /// </summary>
        /// <param name="to">Номер телефона получателя.</param>
        /// <param name="text">Текст сообщения в кодировке UTF-8.</param>
        void SendSms(String to, String text);
    }
}
