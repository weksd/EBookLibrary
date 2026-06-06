using CsvHelper;
using CsvHelper.Configuration;
using EBookLibrary.Models;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;

namespace EBookLibrary.Services
{
    // сервис для импорта и экспорта книг в csv формате
    public class CsvService
    {
        public List<Book> ImportFromCsv(string filePath, int userId)
        {
            var books = new List<Book>();
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);

                var records = csv.GetRecords<BookCsvModel>();
                foreach (var record in records)
                {
                    books.Add(new Book
                    {
                        Title = record.Title,
                        Author = record.Author,
                        Genre = record.Genre,
                        Year = record.Year,
                        FilePath = record.FilePath,
                        IsFavorite = false,
                        UserId = userId
                    });
                }
                Logger.LogInfo($"Импортировано {books.Count} книг из CSV");
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при импорте из CSV", ex);
                throw;
            }
            return books;
        }

        public void ExportToCsv(string filePath, List<Book> books)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                };

                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, config);

                var records = new List<BookCsvModel>();
                foreach (var book in books)
                {
                    records.Add(new BookCsvModel
                    {
                        Title = book.Title,
                        Author = book.Author,
                        Genre = book.Genre,
                        Year = book.Year,
                        FilePath = book.FilePath
                    });
                }

                csv.WriteRecords(records);
                Logger.LogInfo($"Экспортировано {books.Count} книг в CSV");
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при экспорте в CSV", ex);
                throw;
            }
        }

        private class BookCsvModel
        {
            public string Title { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string Genre { get; set; } = string.Empty;
            public int Year { get; set; }
            public string FilePath { get; set; } = string.Empty;
        }
    }
}