using LibraryApp.DataAccess;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LibraryApp
{
    public partial class AddEditBookWindow : Window
    {
        private int _bookId = 0;
        private bool _isBookAvailable = true;
        private DataTable _authorsTable; // Поле для хранения полной таблицы авторов (для фильтрации)

        public AddEditBookWindow(int bookId = 0)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadAuthors();
            if (_bookId > 0)
                LoadBookData();
            else
                UpdateStatusDisplay(true);
        }

        // Загрузка авторов
        private void LoadAuthors()
        {
            string query = "SELECT Id, Name FROM Authors ORDER BY Name";
            _authorsTable = DbHelper.GetData(query);
            cbAuthors.ItemsSource = _authorsTable.DefaultView;
        }

        // Обработчик поиска по авторам (фильтрация + фикс курсора)
        private void CbAuthors_KeyUp(object sender, KeyEventArgs e)
        {
            if (_authorsTable == null) return;

            string filterText = cbAuthors.Text;

            if (string.IsNullOrEmpty(filterText))
            {
                _authorsTable.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                string safeFilter = filterText.Replace("'", "''");
                _authorsTable.DefaultView.RowFilter = $"Name LIKE '%{safeFilter}%'";
            }

            if (!string.IsNullOrEmpty(filterText))
            {
                cbAuthors.IsDropDownOpen = true;

                // Фикс курсора — чтобы текст не выделялся при вводе
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    var textBox = cbAuthors.Template?.FindName("PART_EditableTextBox", cbAuthors) as TextBox;
                    if (textBox != null)
                    {
                        textBox.CaretIndex = textBox.Text.Length;
                        textBox.Select(textBox.Text.Length, 0);
                    }
                }));
            }
        }

        // Загрузка книг
        private void LoadBookData()
        {
            string query = "SELECT Id, Title, AuthorId, Publisher, Year, Genre, IsAvailable, IsAdult FROM Books WHERE Id = @Id";
            DataTable dt = DbHelper.GetData(query, new SqlParameter("@Id", _bookId));
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                txtTitle.Text = row["Title"].ToString();
                txtPublisher.Text = row["Publisher"].ToString();
                txtYear.Text = row["Year"].ToString();
                txtGenre.Text = row["Genre"]?.ToString();

                _isBookAvailable = (bool)row["IsAvailable"];
                UpdateStatusDisplay(_isBookAvailable);

                chkIsAdult.IsChecked = row["IsAdult"] != DBNull.Value && (bool)row["IsAdult"];

                cbAuthors.SelectedValue = row["AuthorId"];
            }
        }

        // Проверка на выдачу книги
        private void UpdateStatusDisplay(bool isAvailable)
        {
            if (isAvailable)
            {
                txtStatus.Text = "✓ Доступна";
                txtStatus.Foreground = Brushes.Green;
            }
            else
            {
                txtStatus.Text = "✗ Выдана";
                txtStatus.Foreground = Brushes.Red;
            }
        }

        // Обработка кнопки сохранить
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string publisher = txtPublisher.Text.Trim();
            string genre = txtGenre.Text.Trim();

            if (!int.TryParse(txtYear.Text, out int year))
                year = 0;

            int authorId = cbAuthors.SelectedValue != null ? (int)cbAuthors.SelectedValue : 0;
            bool isAdult = chkIsAdult.IsChecked == true;

            if (string.IsNullOrEmpty(title) || authorId == 0 || string.IsNullOrEmpty(publisher))
            {
                MessageBox.Show("Введите название книги, автора и издательство.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_bookId == 0) // Добавляем новую книгу
            {
                string insert = "INSERT INTO Books (Title, AuthorId, Publisher, Year, IsAvailable, Genre, IsAdult) " +
                                "VALUES (@Title, @AuthorId, @Publisher, @Year, @IsAvailable, @Genre, @IsAdult)";
                DbHelper.Execute(insert,
                    new SqlParameter("@Title", title),
                    new SqlParameter("@AuthorId", authorId),
                    new SqlParameter("@Publisher", publisher),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@IsAvailable", true),
                    new SqlParameter("@Genre", genre),
                    new SqlParameter("@IsAdult", isAdult));
            }
            else // Редактируем существующую
            {
                string update = "UPDATE Books SET Title=@Title, AuthorId=@AuthorId, Publisher=@Publisher, Year=@Year, " +
                                "IsAvailable=@IsAvailable, Genre=@Genre, IsAdult=@IsAdult WHERE Id=@Id";
                DbHelper.Execute(update,
                    new SqlParameter("@Title", title),
                    new SqlParameter("@AuthorId", authorId),
                    new SqlParameter("@Publisher", publisher),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@IsAvailable", _isBookAvailable),
                    new SqlParameter("@Genre", genre),
                    new SqlParameter("@IsAdult", isAdult),
                    new SqlParameter("@Id", _bookId));
            }

            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}