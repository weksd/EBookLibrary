using System;
using System.IO;

namespace EBookLibrary.Services
{
    // сервис для записи ошибок и событий в текстовый файл
    public static class Logger
    {
        private static readonly string logFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "app_log.txt");

        public static void LogError(string message, Exception? ex = null)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
                if (ex != null)
                {
                    logMessage += $"\nException: {ex}";
                }
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // если не удалось записать лог, игнорируем
            }
        }

        public static void LogInfo(string message)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // если не удалось записать лог, игнорируем
            }
        }
    }
}