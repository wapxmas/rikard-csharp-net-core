using Microsoft.Extensions.Options;
using RikardLib.AspLog;
using RikardLib.AspSmsRu.SmsRu;
using RikardLib.AspSmsRu.SmsRu.Options;
using RikardLib.Parallel;
using SmsRu.Enumerations;
using SmsRu.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmsRu
{
    /// <summary>
    /// Класс для работы с SMS.RU API. ISmsProvider - интерфейс, в котором описаны сигнатуры методов для работы с API.
    /// </summary>
    public class SmsRuService : ISmsRuService
    {
        /*
         * Проект открытый, можно использовать как угодно. Сохраняйте только авторство.
         * Официальная документация по API - http://sms.ru/?panel=api&subpanel=method&show=sms/send.
         * Разработчик - gennadykarasev@gmail.com. В случае, если что-то не работает, то писать на эту почту.
         * 
         * Для работы с методами класса, нужно указать в app.config значения для переменных, которые используются в коде ниже.
         * Следите за балансом. Если баланса не хватит, чтобы отправить на все номера - сообщение будет уничтожено (его не получит никто).
         *
         */

        // Адреса-константы для работы с API
        const String tokenUrl = "http://sms.ru/auth/get_token";
        const String sendUrl = "http://sms.ru/sms/send";
        const String statusUrl = "http://sms.ru/sms/status";
        const String costUrl = "http://sms.ru/sms/cost";
        const String balanceUrl = "http://sms.ru/my/balance";
        const String limitUrl = "http://sms.ru/my/limit";
        const String sendersUrl = "http://sms.ru/my/senders";
        const String authUrl = "http://sms.ru/auth/check";

        private readonly IAspLogger logger;
        private readonly string login;
        private readonly string password;
        private readonly string apiId;
        private readonly string partnerId;
        private readonly string from;
        private readonly bool translit;
        private readonly bool test;
        private readonly ParallelGatherSingle<SmsMessage> Worker;

        public SmsRuService(IAspLogger logger, IOptions<SmsRuOptions> options)
        {
            this.logger = logger;
            this.login = options.Value.Login;
            this.password = options.Value.Password;
            this.apiId = options.Value.ApiId;
            this.partnerId = options.Value.PartnerId;
            this.from = options.Value.From;
            this.translit = options.Value.Translit;
            this.test = options.Value.Test;

            Worker = new ParallelGatherSingle<SmsMessage>(DoSendSms);

            logger.Info($"SmsRuService has been initialized.");
        }

        private void DoSendSms(SmsMessage sms)
        {
            logger.Info("Sending a sms message.");

            Task.Run(async () => await Send(sms.To, sms.Text)).GetAwaiter().GetResult();
        }

        public void SendSms(String to, String text)
        {
            Worker.AddData(new SmsMessage { To = to, Text = text });
        }

        private async Task<String> Send(String to, String text)
        {
            return await Send(from, new String[] { to }, text, DateTime.MinValue, EnumAuthenticationTypes.Strong);
        }
        
        private async Task<String> Send(String from, String[] to, String text, DateTime dateTime, EnumAuthenticationTypes authType)
        {
            // TODO: Нужно проверить хватит ли баланса. Баланса не хватит, чтобы отправить на все номера - сообщение будет уничтожено (его не получит никто).
            String result = String.Empty;

            if (to.Length < 1)
            {
                logger.Error("SmsRu: to=Неверные входные данные - массив пуст.");
                return result;
            }
            if (to.Length > 100)
            {
                logger.Error("SmsRu: to=Неверные входные данные - слишком много элементов (больше 100) в массиве.");
                return result;
            }
            if (dateTime == DateTime.MinValue)
                dateTime = DateTime.Now;
            // Лишнее, не надо генерировать это исключение. Если время меньше текущего времени, сообщение отправляется моментально - правило на сервере.
            // if ((DateTime.Now - dateTime).Days > new TimeSpan(7, 0, 0, 0).Days)
            //    throw new ArgumentOutOfRangeException("dateTime", "Неверные входные данные - должно быть не больше 7 дней с момента подачи запроса.");

            String auth = String.Empty;
            String parameters = String.Empty;
            String answer = String.Empty;
            String recipients = String.Empty;
            String token = String.Empty;

            foreach (String item in to)
            {
                recipients += item + ",";
            }

            recipients = recipients.Substring(0, recipients.Length - 1);

            logger.Info($"SmsRu: Отправка СМС получателям: {recipients}");

            try
            {
                token = await GetToken();

                String sha512 = HashCodeHelper.GetSHA512Hash(String.Format("{0}{1}", password, token)).ToLower();
                String sha512wapi = HashCodeHelper.GetSHA512Hash(String.Format("{0}{1}{2}", password, token, apiId)).ToLower();

                if (authType == EnumAuthenticationTypes.Simple)
                    auth = String.Format("api_id={0}", apiId);
                if (authType == EnumAuthenticationTypes.Strong)
                    auth = String.Format("login={0}&token={1}&sha512={2}", login, token, sha512);
                if (authType == EnumAuthenticationTypes.StrongApi)
                    auth = String.Format("login={0}&token={1}&sha512={2}", login, token, sha512wapi);

                parameters = String.Format("{0}&to={1}&text={2}&from={3}", auth, recipients, text, from);
                if (dateTime != DateTime.MinValue)
                    parameters += "&time=" + TimeHelper.GetUnixTime(dateTime);
                if (!string.IsNullOrWhiteSpace(partnerId))
                    parameters += "&partner_id=" + partnerId;
                if (translit == true)
                    parameters += "&translit=1";
                if (test == true)
                    parameters += "&test=1";
                logger.Info($"SmsRu: Запрос: {parameters}");

                WebRequest request = WebRequest.Create(sendUrl);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                Byte[] bytes = Encoding.UTF8.GetBytes(parameters);
                request.Headers["ContentLength"] = bytes.Length.ToString();
                using (Stream os = await request.GetRequestStreamAsync())
                {
                    os.Write(bytes, 0, bytes.Length);
                }

                using (WebResponse resp = await request.GetResponseAsync())
                {
                    if (resp == null) return null;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        answer = sr.ReadToEnd().Trim();
                    }
                }

                logger.Info($"SmsRu: Ответ: {answer}");

                String[] lines = answer.Split(new String[] { "\n" }, StringSplitOptions.None);
                if (Convert.ToInt32(lines[0]) == Convert.ToInt32(ResponseOnSendRequest.MessageAccepted))
                {
                    result = answer;
                }
                else
                {
                    result = String.Empty;
                }
            }
            catch (Exception ex)
            {
                logger.Error("SmsRu: Возникла непонятная ошибка", ex);
            }

            return result;
        }

        public async Task<String> GetToken()
        {
            String result = String.Empty;

            try
            {
                WebRequest request = WebRequest.Create(tokenUrl);
                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream != null)
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                result = sr.ReadToEnd();
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Возникла ошибка при получении токена по адресу http://sms.ru/auth/get_token.", ex);
            }

            return result;
        }
    }
}