using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Data.Sqlite;

namespace EBookLibrary.Services
{
    // сервис для резервного копирования и восстановления базы данных
    public class BackupService
    {
        private readonly string dbPath;

        public BackupService()
        {
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library.db");
        }

        public void CreateBackup(string backupPath)
        {
            try
            {
                // закрываем все соединения с базой
                SqliteConnection.ClearAllPools();

                using var zip = ZipFile.Open(backupPath, ZipArchiveMode.Create);
                zip.CreateEntryFromFile(dbPath, "library.db");

                Logger.LogInfo($"Создана резервная копия: {backupPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при создании резервной копии", ex);
                throw;
            }
        }

        public void RestoreFromBackup(string backupPath)
        {
            try
            {
                SqliteConnection.ClearAllPools();

                // удаляем текущую базу
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }

                // извлекаем базу из архива
                using var zip = ZipFile.OpenRead(backupPath);
                var entry = zip.GetEntry("library.db");
                if (entry != null)
                {
                    entry.ExtractToFile(dbPath, true);
                }

                Logger.LogInfo($"База данных восстановлена из: {backupPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при восстановлении из резервной копии", ex);
                throw;
            }
        }
    }
}