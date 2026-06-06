using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace EBookLibrary.Services
{
    // сервис для работы с базой данных sqlite
    public class DatabaseService
    {
        private readonly string connectionString;

        public DatabaseService()
        {
            string dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "library.db");
            connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                // создание таблицы пользователей
                string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Login TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL
                    );";

                // создание таблицы книг
                string createBooksTable = @"
                    CREATE TABLE IF NOT EXISTS Books (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Author TEXT NOT NULL,
                        Genre TEXT NOT NULL,
                        Year INTEGER NOT NULL,
                        FilePath TEXT NOT NULL,
                        IsFavorite INTEGER NOT NULL DEFAULT 0,
                        UserId INTEGER NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );";

                using var command = new SqliteCommand(createUsersTable, connection);
                command.ExecuteNonQuery();

                command.CommandText = createBooksTable;
                command.ExecuteNonQuery();

                Logger.LogInfo("База данных инициализирована успешно");
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при инициализации базы данных", ex);
                throw;
            }
        }

        public SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}