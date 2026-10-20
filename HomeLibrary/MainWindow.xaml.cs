using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Models;
using HomeLibrary.Services;

namespace HomeLibrary
{
    public partial class MainWindow : Window
    {
        private readonly LibraryService _service = new();
        private Book? _selected;

        public MainWindow()
        {
            InitializeComponent();
            _service.Load();
            Loaded += (_, _) => Refresh();
        }

        // ================== 1. Добавление ==================
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate()) return;

                _service.Add(new Book
                {
                    Author = TxtAuthor.Text.Trim(),
                    Title = TxtTitle.Text.Trim(),
                    Year = int.Parse(TxtYear.Text),
                    Genre = TxtGenre.Text.Trim(),
                    Status = (BookStatus)CmbStatus.SelectedIndex,
                    IsFavorite = ChkFav.IsChecked == true
                });

                _service.Save();
                Refresh();
                ClearForm();
            }
            catch (FormatException ex)
            {
                MessageBox.Show(
                    "Год должен быть целым числом.\n" +
                    $"Техническая информация: {ex.Message}",
                    "Неверный формат",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show(
                    $"Недопустимое значение: {ex.Message}",
                    "Ошибка диапазона",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Непредвиденная ошибка при добавлении: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ================== 2. Редактирование ==================
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selected == null) { Warn("Выберите книгу."); return; }
                if (!Validate()) return;

                _selected.Author = TxtAuthor.Text.Trim();
                _selected.Title = TxtTitle.Text.Trim();
                _selected.Year = int.Parse(TxtYear.Text);
                _selected.Genre = TxtGenre.Text.Trim();
                _selected.Status = (BookStatus)CmbStatus.SelectedIndex;
                _selected.IsFavorite = ChkFav.IsChecked == true;

                _service.Save();
                Refresh();
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Год должен быть числом. {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================== 3. Удаление ==================
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selected == null) { Warn("Выберите книгу."); return; }

                if (MessageBox.Show($"Удалить «{_selected.Title}»?", "Подтверждение",
                        MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

                _service.Remove(_selected);
                _service.Save();
                _selected = null;
                ClearForm();
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================== 4. Очистка формы ==================
        private void BtnClear_Click(object sender, RoutedEventArgs e) => ClearForm();

        // ================== 5-6-7. Поиск/фильтр/сортировка ==================
        private void Filter_Changed(object sender, object e) => Refresh();

        // ================== 8. Выбор книги ==================
        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = GridBooks.SelectedItem as Book;
            if (_selected == null) return;

            TxtAuthor.Text = _selected.Author;
            TxtTitle.Text = _selected.Title;
            TxtYear.Text = _selected.Year.ToString();
            TxtGenre.Text = _selected.Genre;
            CmbStatus.SelectedIndex = (int)_selected.Status;
            ChkFav.IsChecked = _selected.IsFavorite;
        }

        // ================== 9. Refresh ==================
        private void Refresh()
        {
            if (GridBooks == null || CmbFilter == null || CmbSort == null || TxtStats == null)
                return;

            var q = _service.Search(TxtSearch.Text);

            var filter = (CmbFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";
            q = filter switch
            {
                "Хочу прочитать" => q.Where(b => b.Status == BookStatus.WantToRead),
                "Читаю" => q.Where(b => b.Status == BookStatus.Reading),
                "Прочитано" => q.Where(b => b.Status == BookStatus.Finished),
                "Только избранное" => q.Where(b => b.IsFavorite),
                _ => q
            };

            var sort = (CmbSort.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "По автору";
            q = sort switch
            {
                "По названию" => q.OrderBy(b => b.Title),
                "По году" => q.OrderByDescending(b => b.Year),
                _ => q.OrderBy(b => b.Author)
            };

            var list = q.ToList();
            GridBooks.ItemsSource = list;

            int total = _service.Books.Count;
            int done = _service.Books.Count(b => b.Status == BookStatus.Finished);
            double avg = _service.Books.Any(b => b.Year > 0)
                ? _service.Books.Where(b => b.Year > 0).Average(b => b.Year) : 0;

            TxtStats.Text = $"Всего: {total}\nПрочитано: {done}\n" +
                            $"Средний год: {avg:F0}\nПоказано: {list.Count}";
        }

        // ================== 10. Валидация ==================
        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(TxtAuthor.Text) ||
                string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                Warn("Автор и название обязательны.");
                return false;
            }

            int year;
            try { year = int.Parse(TxtYear.Text); }
            catch (FormatException)
            {
                throw new FormatException("Год должен быть целым числом.");
            }

            if (year < 0 || year > DateTime.Now.Year + 1)
                throw new ArgumentOutOfRangeException(
                    nameof(year), year,
                    $"Год должен быть от 0 до {DateTime.Now.Year + 1}.");

            return true;
        }

        // ================== 11. Очистка полей ==================
        private void ClearForm()
        {
            TxtAuthor.Clear();
            TxtTitle.Clear();
            TxtYear.Clear();
            TxtGenre.Clear();
            CmbStatus.SelectedIndex = 0;
            ChkFav.IsChecked = false;
            _selected = null;
        }

        // ================== 12. Warn ==================
        private void Warn(string msg) =>
            MessageBox.Show(msg, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}