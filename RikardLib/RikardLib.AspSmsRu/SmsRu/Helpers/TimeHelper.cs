using System;

namespace SmsRu.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с классом System.DateTime.
    /// </summary>
    class TimeHelper
    {
        /// <summary>
        /// Получить текущее время в UNIX-формате.
        /// </summary>
        /// <returns>Время в UNIX-формате.</returns>
        public static Int32 GetCurrentUnixTime()
        {            
            return GetUnixTime(DateTime.Now);
        }

        /// <summary>
        /// Получить время на определённый момент в UNIX-формате.
        /// </summary>
        /// <param name="dateTime">Требуемый момент времени.</param>
        /// <returns>Время в UNIX-формате.</returns>
        public static Int32 GetUnixTime(DateTime dateTime)
            => (Int32)((dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
    }
}
