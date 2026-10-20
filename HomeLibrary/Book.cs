using System;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

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

        [JsonIgnore]
        public string StatusText => Status switch
        {
            BookStatus.WantToRead => "Хочу прочитать",
            BookStatus.Reading => "Читаю",
            BookStatus.Finished => "Прочитано",
            _ => "Неизвестно"          // ← защита от некорректного значения
        };

        [JsonIgnore]
        public Brush StatusColor
        {
            get
            {
                try
                {
                    return Status switch
                    {
                        BookStatus.WantToRead => new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)),
                        BookStatus.Reading => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
                        BookStatus.Finished => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
                        _ => Brushes.Gray
                    };
                }
                catch (Exception)
                {
                    // Никогда не должно сработать, но защищаем UI
                    return Brushes.Gray;
                }
            }
        }

        [JsonIgnore]
        public Visibility FavVisibility => IsFavorite ? Visibility.Visible : Visibility.Hidden;
    }
}