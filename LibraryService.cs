using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HomeLibrary.Models;

namespace HomeLibrary.Services
{
    public class LibraryService
    {
        private readonly string _path = "library.json";

        public List<Book> Books { get; private set; } = new();

        public void Load()
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                Books = JsonSerializer.Deserialize<List<Book>>(json) ?? new();
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(Books,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }

        public void Add(Book b) => Books.Add(b);
        public void Remove(Book b) => Books.Remove(b);

        public IEnumerable<Book> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Books;
            q = q.ToLower();
            return Books.Where(b =>
                b.Author.ToLower().Contains(q) ||
                b.Title.ToLower().Contains(q));
        }
    }
}