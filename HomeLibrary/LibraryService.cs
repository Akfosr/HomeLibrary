using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using HomeLibrary.Models;

namespace HomeLibrary.Services
{
    public class LibraryService
    {
        private readonly string _path = "library.json";

        public List<Book> Books { get; private set; } = new();

        public void Load()
        {
            try
            {
                if (!File.Exists(_path))
                    return;                     // Первый запуск — норм

                var json = File.ReadAllText(_path);

                if (string.IsNullOrWhiteSpace(json))
                    return;                     // Пустой файл — не ошибка

                Books = JsonSerializer.Deserialize<List<Book>>(json) ?? new();
            }
            catch (JsonException ex)
            {
                MessageBox.Show(
                    "Файл library.json повреждён. Будет создан новый каталог.\n\n" +
                    $"Техническая информация: {ex.Message}",
                    "Ошибка чтения данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Books = new List<Book>();
                TryBackupBrokenFile();
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    "Не удалось прочитать файл library.json.\n" +
                    "Возможно, он занят другим приложением.\n\n" +
                    $"Техническая информация: {ex.Message}",
                    "Ошибка доступа",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(
                    "Нет прав на чтение файла library.json.\n" +
                    "Попробуйте запустить приложение от имени администратора.\n\n" +
                    $"Техническая информация: {ex.Message}",
                    "Отказано в доступе",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Непредвиденная ошибка при загрузке: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Books,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить library.json.\n" +
                    "Проверьте свободное место на диске.\n\n" +
                    $"Техническая информация: {ex.Message}",
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(
                    "Нет прав на запись library.json.\n" +
                    "Попробуйте запустить приложение от имени администратора.\n\n" +
                    $"Техническая информация: {ex.Message}",
                    "Отказано в доступе",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TryBackupBrokenFile()
        {
            try
            {
                var backup = $"{_path}.broken_{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(_path, backup, overwrite: true);
            }
            catch { /* пропускаем — это не критично */ }
        }

        public void Add(Book b)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            Books.Add(b);
        }

        public void Remove(Book b)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            Books.Remove(b);
        }

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