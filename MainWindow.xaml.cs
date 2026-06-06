using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using Microsoft.Win32;
using EBookLibrary.Models;
using EBookLibrary.Services;
using EBookLibrary.Views;

namespace EBookLibrary
{
    // конвертер для отображения статуса избранного
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFavorite && isFavorite)
            {
                return "Gold";
            }
            return "Gray";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        private readonly User currentUser;
        private readonly DatabaseService databaseService;
        private readonly BookService bookService;
        private readonly CsvService csvService;
        private readonly BackupService backupService;
        private List<Book> allBooks;
        private bool showingFavorites = false;

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            databaseService = new DatabaseService();
            bookService = new BookService(databaseService, user.Id);
            csvService = new CsvService();
            backupService = new BackupService();

            // добавляем конвертер в ресурсы окна
            this.Resources.Add("BoolToColorConverter", new BoolToColorConverter());

            UserInfoText.Text = $"Пользователь: {user.Login}";
            LoadBooks();
        }

        private void LoadBooks()
        {
            if (showingFavorites)
            {
                allBooks = bookService.GetFavoriteBooks();
            }
            else
            {
                allBooks = bookService.GetAllBooks();
            }
            BooksDataGrid.ItemsSource = allBooks;
        }

        private Book? GetSelectedBook()
        {
            return BooksDataGrid.SelectedItem as Book;
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new BookEditWindow();
            editWindow.ShowDialog();

            if (editWindow.IsSaved && editWindow.BookResult != null)
            {
                bookService.AddBook(editWindow.BookResult);
                LoadBooks();
            }
        }

        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = GetSelectedBook();
            if (selectedBook == null)
            {
                MessageBox.Show("Выберите книгу для редактирования", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new BookEditWindow(selectedBook);
            editWindow.ShowDialog();

            if (editWindow.IsSaved && editWindow.BookResult != null)
            {
                bookService.UpdateBook(editWindow.BookResult);
                LoadBooks();
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = GetSelectedBook();
            if (selectedBook == null)
            {
                MessageBox.Show("Выберите книгу для удаления", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить книгу \"{selectedBook.Title}\"?",
                                         "Подтверждение", MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                bookService.DeleteBook(selectedBook.Id);
                LoadBooks();
            }
        }

        private void FavoriteBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = GetSelectedBook();
            if (selectedBook == null)
            {
                MessageBox.Show("Выберите книгу", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bookService.ToggleFavorite(selectedBook.Id);
            LoadBooks();
        }

        private void ReadBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = GetSelectedBook();
            if (selectedBook == null)
            {
                MessageBox.Show("Выберите книгу для чтения", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var process = new Process();
                process.StartInfo.FileName = selectedBook.FilePath;
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при открытии файла книги", ex);
                MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadBooks();
                return;
            }

            allBooks = bookService.SearchBooks(searchText);
            BooksDataGrid.ItemsSource = allBooks;
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text.Trim()))
            {
                LoadBooks();
            }
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            showingFavorites = !showingFavorites;
            if (showingFavorites)
            {
                ((System.Windows.Controls.Button)sender).Content = "Все книги";
            }
            else
            {
                ((System.Windows.Controls.Button)sender).Content = "Избранное";
            }
            LoadBooks();
        }

        private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите CSV файл для импорта";
            openFileDialog.Filter = "CSV файлы|*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var importedBooks = csvService.ImportFromCsv(openFileDialog.FileName, currentUser.Id);
                    int addedCount = 0;
                    foreach (var book in importedBooks)
                    {
                        if (bookService.AddBook(book))
                        {
                            addedCount++;
                        }
                    }
                    MessageBox.Show($"Импортировано книг: {addedCount} из {importedBooks.Count}",
                                  "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Сохранить CSV файл";
            saveFileDialog.Filter = "CSV файлы|*.csv";
            saveFileDialog.FileName = $"library_export_{DateTime.Now:yyyyMMdd}.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var books = bookService.GetAllBooks();
                    csvService.ExportToCsv(saveFileDialog.FileName, books);
                    MessageBox.Show($"Экспортировано книг: {books.Count}",
                                  "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Создать резервную копию";
            saveFileDialog.Filter = "ZIP архивы|*.zip";
            saveFileDialog.FileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    backupService.CreateBackup(saveFileDialog.FileName);
                    MessageBox.Show("Резервная копия создана успешно", "Готово",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании копии: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Восстановление из резервной копии заменит все данные. Продолжить?",
                                       "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файл резервной копии";
            openFileDialog.Filter = "ZIP архивы|*.zip";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    backupService.RestoreFromBackup(openFileDialog.FileName);
                    MessageBox.Show("База данных восстановлена. Приложение будет перезапущено.",
                                  "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                    // перезапускаем приложение
                    System.Diagnostics.Process.Start(
                        System.Reflection.Assembly.GetExecutingAssembly().Location);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при восстановлении: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}