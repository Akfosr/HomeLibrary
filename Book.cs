using System;

namespace HomeLibrary.Models
{
    public enum BookStatus
    {
        WantToRead,
        Reading,
        Finished
    }

    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Author { get; set; } = "";
        public string Title { get; set; } = "";
        public int Year { get; set; }
        public string Genre { get; set; } = "";
        public BookStatus Status { get; set; } = BookStatus.WantToRead;
        public bool IsFavorite { get; set; }

        public string StatusText => Status switch
        {
            BookStatus.WantToRead => "Хочу прочитать",
            BookStatus.Reading => "Читаю",
            BookStatus.Finished => "Прочитано",
            _ => "—"
        };
    }
}