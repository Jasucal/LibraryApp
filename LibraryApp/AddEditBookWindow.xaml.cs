using System.Data;
using System.Windows;
using System.Windows.Media;
using LibraryApp.DataAccess;
using System.Data.SqlClient;

namespace LibraryApp
{
    public partial class AddEditBookWindow : Window
    {
        private int _bookId = 0;
        private bool _isBookAvailable = true; // По умолчанию новая книга доступна

        public AddEditBookWindow(int bookId = 0)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadAuthors();
            if (_bookId > 0)
                LoadBookData();
            else
                UpdateStatusDisplay(true); // Новая книга — доступна
        }

        private void LoadAuthors()
        {
            string query = "SELECT Id, Name FROM Authors";
            cbAuthors.ItemsSource = DbHelper.GetData(query).DefaultView;
        }

        private void LoadBookData()
        {
            string query = "SELECT * FROM Books WHERE Id = @Id";
            DataTable dt = DbHelper.GetData(query, new SqlParameter("@Id", _bookId));
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                txtTitle.Text = row["Title"].ToString();
                txtPublisher.Text = row["Publisher"].ToString();
                txtYear.Text = row["Year"].ToString();
                txtGenre.Text = row["Genre"]?.ToString();

                // Сохраняем и показываем статус
                _isBookAvailable = (bool)row["IsAvailable"];
                UpdateStatusDisplay(_isBookAvailable);

                cbAuthors.SelectedValue = row["AuthorId"];
            }
        }

        // Обновление отображение статуса
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

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string publisher = txtPublisher.Text.Trim();
            string genre = txtGenre.Text.Trim();

            if (!int.TryParse(txtYear.Text, out int year))
                year = 0;

            int authorId = cbAuthors.SelectedValue != null ? (int)cbAuthors.SelectedValue : 0;

            if (string.IsNullOrEmpty(title) || authorId == 0 || string.IsNullOrEmpty(publisher))
            {
                MessageBox.Show("Введите название книги, автора и издательство.");
                return;
            }

            // Проверка дубликата
            string checkQuery = "SELECT * FROM Books WHERE Title=@Title AND AuthorId=@AuthorId AND Publisher=@Publisher" +
                                (_bookId > 0 ? " AND Id<>@Id" : "");
            var parameters = _bookId > 0
                ? new SqlParameter[]
                {
                    new SqlParameter("@Title", title),
                    new SqlParameter("@AuthorId", authorId),
                    new SqlParameter("@Publisher", publisher),
                    new SqlParameter("@Id", _bookId)
                }
                : new SqlParameter[]
                {
                    new SqlParameter("@Title", title),
                    new SqlParameter("@AuthorId", authorId),
                    new SqlParameter("@Publisher", publisher)
                };

            DataTable existing = DbHelper.GetData(checkQuery, parameters);
            if (existing.Rows.Count > 0)
            {
                MessageBox.Show("Книга с таким названием, автором и издательством уже существует.");
                return;
            }

            if (_bookId == 0) // Добавляем новую книгу
            {
                string insert = "INSERT INTO Books (Title, AuthorId, Publisher, Year, IsAvailable, Genre) " +
                                "VALUES (@Title, @AuthorId, @Publisher, @Year, @IsAvailable, @Genre)";
                DbHelper.Execute(insert,
                    new SqlParameter("@Title", title),
                    new SqlParameter("@AuthorId", authorId),
                    new SqlParameter("@Publisher", publisher),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@IsAvailable", true), // Новая книга всегда доступна
                    new SqlParameter("@Genre", genre));
            }
            else // Редактируем существующую
            {
                string update = "UPDATE Books SET Title=@Title, AuthorId=@AuthorId, Publisher=@Publisher, Year=@Year, " +
                                "IsAvailable=@IsAvailable, Genre=@Genre WHERE Id=@Id";
                DbHelper.Execute(update,
                    new SqlParameter("@Title", title),
                    new SqlParameter("@AuthorId", authorId),
                    new SqlParameter("@Publisher", publisher),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@IsAvailable", _isBookAvailable), // Сохранение текущего статус
                    new SqlParameter("@Genre", genre),
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