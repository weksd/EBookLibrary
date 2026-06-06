using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using EBookLibrary.Models;

namespace EBookLibrary.Services
{
    // сервис для управления книгами в библиотеке
    public class BookService
    {
        private readonly DatabaseService databaseService;
        private readonly int userId;

        public BookService(DatabaseService dbService, int userId)
        {
            databaseService = dbService;
            this.userId = userId;
        }

        public List<Book> GetAllBooks()
        {
            var books = new List<Book>();
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = "SELECT Id, Title, Author, Genre, Year, FilePath, IsFavorite " +
                            "FROM Books WHERE UserId = @userId ORDER BY Title";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@userId", userId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(new Book
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Genre = reader.GetString(3),
                        Year = reader.GetInt32(4),
                        FilePath = reader.GetString(5),
                        IsFavorite = reader.GetBoolean(6),
                        UserId = userId
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при получении списка книг", ex);
            }
            return books;
        }

        public List<Book> GetFavoriteBooks()
        {
            var books = new List<Book>();
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = "SELECT Id, Title, Author, Genre, Year, FilePath, IsFavorite " +
                            "FROM Books WHERE UserId = @userId AND IsFavorite = 1 ORDER BY Title";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@userId", userId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(new Book
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Genre = reader.GetString(3),
                        Year = reader.GetInt32(4),
                        FilePath = reader.GetString(5),
                        IsFavorite = true,
                        UserId = userId
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при получении избранных книг", ex);
            }
            return books;
        }

        public bool AddBook(Book book)
        {
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = @"INSERT INTO Books (Title, Author, Genre, Year, FilePath, IsFavorite, UserId) 
                              VALUES (@title, @author, @genre, @year, @filePath, @isFavorite, @userId)";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@title", book.Title);
                command.Parameters.AddWithValue("@author", book.Author);
                command.Parameters.AddWithValue("@genre", book.Genre);
                command.Parameters.AddWithValue("@year", book.Year);
                command.Parameters.AddWithValue("@filePath", book.FilePath);
                command.Parameters.AddWithValue("@isFavorite", book.IsFavorite ? 1 : 0);
                command.Parameters.AddWithValue("@userId", userId);
                command.ExecuteNonQuery();

                Logger.LogInfo($"Добавлена книга: {book.Title}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при добавлении книги", ex);
                return false;
            }
        }

        public bool UpdateBook(Book book)
        {
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = @"UPDATE Books SET Title = @title, Author = @author, 
                              Genre = @genre, Year = @year, FilePath = @filePath, 
                              IsFavorite = @isFavorite WHERE Id = @id AND UserId = @userId";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@title", book.Title);
                command.Parameters.AddWithValue("@author", book.Author);
                command.Parameters.AddWithValue("@genre", book.Genre);
                command.Parameters.AddWithValue("@year", book.Year);
                command.Parameters.AddWithValue("@filePath", book.FilePath);
                command.Parameters.AddWithValue("@isFavorite", book.IsFavorite ? 1 : 0);
                command.Parameters.AddWithValue("@id", book.Id);
                command.Parameters.AddWithValue("@userId", userId);
                command.ExecuteNonQuery();

                Logger.LogInfo($"Обновлена книга: {book.Title}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при обновлении книги", ex);
                return false;
            }
        }

        public bool DeleteBook(int bookId)
        {
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = "DELETE FROM Books WHERE Id = @id AND UserId = @userId";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@id", bookId);
                command.Parameters.AddWithValue("@userId", userId);
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Logger.LogInfo($"Удалена книга с ID: {bookId}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при удалении книги", ex);
                return false;
            }
        }

        public bool ToggleFavorite(int bookId)
        {
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = "UPDATE Books SET IsFavorite = CASE WHEN IsFavorite = 1 THEN 0 ELSE 1 END " +
                            "WHERE Id = @id AND UserId = @userId";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@id", bookId);
                command.Parameters.AddWithValue("@userId", userId);
                command.ExecuteNonQuery();

                Logger.LogInfo($"Изменен статус избранного для книги с ID: {bookId}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при изменении статуса избранного", ex);
                return false;
            }
        }

        public List<Book> SearchBooks(string searchText)
        {
            var books = new List<Book>();
            try
            {
                using var connection = databaseService.GetConnection();
                string sql = @"SELECT Id, Title, Author, Genre, Year, FilePath, IsFavorite 
                              FROM Books 
                              WHERE UserId = @userId AND (Title LIKE @search OR Author LIKE @search OR Genre LIKE @search)
                              ORDER BY Title";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@search", $"%{searchText}%");

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(new Book
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Genre = reader.GetString(3),
                        Year = reader.GetInt32(4),
                        FilePath = reader.GetString(5),
                        IsFavorite = reader.GetBoolean(6),
                        UserId = userId
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при поиске книг", ex);
            }
            return books;
        }
    }
}