namespace EBookLibrary.Models
{
    // модель книги, содержит все поля для библиотеки
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Year { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public int UserId { get; set; }
    }
}