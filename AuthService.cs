using System;
using Microsoft.Data.Sqlite;
using EBookLibrary.Models;

namespace EBookLibrary.Services
{
    // сервис для авторизации и регистрации пользователей
    public class AuthService
    {
        private readonly DatabaseService databaseService;

        public AuthService(DatabaseService dbService)
        {
            databaseService = dbService;
        }

        public User? Login(string login, string password)
        {
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = "SELECT Id, Login, PasswordHash FROM Users WHERE Login = @login";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@login", login);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string passwordHash = reader.GetString(2);
                    if (BCrypt.Net.BCrypt.Verify(password, passwordHash))
                    {
                        Logger.LogInfo($"Пользователь {login} вошел в систему");
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Login = reader.GetString(1),
                            PasswordHash = passwordHash
                        };
                    }
                }
                Logger.LogInfo($"Неудачная попытка входа для пользователя {login}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при входе в систему", ex);
                return null;
            }
        }

        public bool Register(string login, string password)
        {
            try
            {
                using var connection = databaseService.GetConnection();
                string checkSql = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                using var checkCommand = new SqliteCommand(checkSql, connection);
                checkCommand.Parameters.AddWithValue("@login", login);

                long count = (long)checkCommand.ExecuteScalar()!;
                if (count > 0)
                {
                    return false; // пользователь уже существует
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                string insertSql = "INSERT INTO Users (Login, PasswordHash) VALUES (@login, @hash)";
                using var insertCommand = new SqliteCommand(insertSql, connection);
                insertCommand.Parameters.AddWithValue("@login", login);
                insertCommand.Parameters.AddWithValue("@hash", passwordHash);
                insertCommand.ExecuteNonQuery();

                Logger.LogInfo($"Зарегистрирован новый пользователь: {login}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при регистрации пользователя", ex);
                return false;
            }
        }

        public bool HasAnyUser()
        {
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = "SELECT COUNT(*) FROM Users";
                using var command = new SqliteCommand(sql, connection);
                long count = (long)command.ExecuteScalar()!;
                return count > 0;
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при проверке наличия пользователей", ex);
                return false;
            }
        }
    }
}