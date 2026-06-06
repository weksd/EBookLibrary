using System.Windows;
using Microsoft.Win32;
using EBookLibrary.Models;

namespace EBookLibrary.Views
{
    public partial class BookEditWindow : Window
    {
        public Book BookResult { get; private set; }
        public bool IsSaved { get; private set; }
        private Book? editingBook;

        // конструктор для добавления новой книги
        public BookEditWindow()
        {
            InitializeComponent();
            IsSaved = false;
            TitleText.Text = "Добавление книги";
        }

        // конструктор для редактирования существующей книги
        public BookEditWindow(Book book) : this()
        {
            editingBook = book;
            TitleText.Text = "Редактирование книги";
            TitleTextBox.Text = book.Title;
            AuthorTextBox.Text = book.Author;
            GenreTextBox.Text = book.Genre;
            YearTextBox.Text = book.Year.ToString();
            FilePathTextBox.Text = book.FilePath;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файл книги";
            openFileDialog.Filter = "Все поддерживаемые форматы|*.pdf;*.epub;*.fb2;*.txt;*.doc;*.docx|Все файлы|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string genre = GenreTextBox.Text.Trim();
            string yearText = YearTextBox.Text.Trim();
            string filePath = FilePathTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) ||
                string.IsNullOrEmpty(genre) || string.IsNullOrEmpty(filePath))
            {
                ErrorMessage.Text = "Заполните все поля";
                return;
            }

            if (!int.TryParse(yearText, out int year))
            {
                ErrorMessage.Text = "Введите корректный год";
                return;
            }

            BookResult = new Book
            {
                Title = title,
                Author = author,
                Genre = genre,
                Year = year,
                FilePath = filePath,
                IsFavorite = editingBook?.IsFavorite ?? false
            };

            if (editingBook != null)
            {
                BookResult.Id = editingBook.Id;
            }

            IsSaved = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}