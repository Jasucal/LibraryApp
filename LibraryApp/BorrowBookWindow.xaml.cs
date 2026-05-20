using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LibraryApp.DataAccess;
using System.Data.SqlClient;

namespace LibraryApp
{
    public partial class BorrowBookWindow : Window
    {
        private DataTable readersTable;
        private DataTable booksTable;
        private int _staffId;
        public BorrowBookWindow(int staffId)
        {
            InitializeComponent();
            _staffId = staffId;
            LoadReaders();
            LoadBooks();
            dpReturnDate.SelectedDate = DateTime.Now.AddDays(14);
        }

        private void LoadReaders()
        {
            string query = @"
                SELECT Id, FullName, DateOfBirth, Email,
                    FullName + ' (' + Email + ' | ' + CONVERT(varchar, DateOfBirth, 104) + ')' AS DisplayName
                FROM Readers
                WHERE Role <> 1";

            readersTable = DbHelper.GetData(query);

            cbReaders.ItemsSource = readersTable.DefaultView;
            cbReaders.DisplayMemberPath = "DisplayName";
            cbReaders.SelectedValuePath = "Id";
            cbReaders.SelectedIndex = -1;
            cbReaders.Text = string.Empty;
        }

        private void LoadBooks()
        {
            string query = @"
                SELECT b.Id, + 'ID ' + 
                    CAST(b.Id AS varchar) + ' | ' + b.Title + ' (' + a.Name + ', ' + b.Publisher + ')' AS DisplayTitle, b.IsAdult
                FROM Books b
                INNER JOIN Authors a ON b.AuthorId = a.Id
                WHERE b.IsAvailable = 1";

            booksTable = DbHelper.GetData(query);

            cbBooks.ItemsSource = booksTable.DefaultView;
            cbBooks.DisplayMemberPath = "DisplayTitle";  // Показываем форматированное название
            cbBooks.SelectedValuePath = "Id";
            cbBooks.SelectedIndex = -1;
            cbBooks.Text = string.Empty;
        }

        // Вспомогательный метод: расчёт возраста
        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                age--;
            return age;
        }

        private bool CanIssueBook(int bookId, int readerId, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Получаем рейтинг для книги
            string bookQuery = "SELECT IsAdult FROM Books WHERE Id = @Id";
            bool isAdultBook = Convert.ToBoolean(
                DbHelper.GetData(bookQuery, new SqlParameter("@Id", bookId)).Rows[0]["IsAdult"]);

            // Если книга не 18+, всё ок
            if (!isAdultBook)
                return true;

            // Для 18+ книги проверяем возраст читателя
            string readerQuery = "SELECT FullName, DateOfBirth FROM Readers WHERE Id = @Id";
            var readerRow = DbHelper.GetData(readerQuery, new SqlParameter("@Id", readerId)).Rows[0];

            DateTime readerDob = Convert.ToDateTime(readerRow["DateOfBirth"]);
            int readerAge = CalculateAge(readerDob);

            if (readerAge < 18)
            {
                errorMessage = $"⛔ Невозможно выдать книгу 18+. Читателю \"{readerRow["FullName"]}\" всего {readerAge} лет.";
                return false;
            }

            return true;
        }

        // Поиск читателей
        private void CbReaders_KeyUp(object sender, KeyEventArgs e)
        {
            if (readersTable == null) return;

            string filterText = cbReaders.Text;

            if (string.IsNullOrEmpty(filterText))
            {
                readersTable.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                string safeFilter = filterText.Replace("'", "''");
                readersTable.DefaultView.RowFilter = "FullName LIKE '%" + safeFilter + "%'";
            }

            if (!string.IsNullOrEmpty(filterText))
            {
                cbReaders.IsDropDownOpen = true;

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    var textBox = cbReaders.Template.FindName("PART_EditableTextBox", cbReaders) as TextBox;
                    if (textBox != null)
                    {
                        textBox.CaretIndex = textBox.Text.Length;
                        textBox.Select(textBox.Text.Length, 0);
                    }
                }));
            }
        }

        // Поиск книг
        private void CbBooks_KeyUp(object sender, KeyEventArgs e)
        {
            if (booksTable == null) return;

            string filterText = cbBooks.Text;

            if (string.IsNullOrEmpty(filterText))
            {
                booksTable.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                string safeFilter = filterText.Replace("'", "''");
                booksTable.DefaultView.RowFilter = "DisplayTitle LIKE '%" + safeFilter + "%'";
            }

            if (!string.IsNullOrEmpty(filterText))
            {
                cbBooks.IsDropDownOpen = true;

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    var textBox = cbBooks.Template.FindName("PART_EditableTextBox", cbBooks) as TextBox;
                    if (textBox != null)
                    {
                        textBox.CaretIndex = textBox.Text.Length;
                        textBox.Select(textBox.Text.Length, 0);
                    }
                }));
            }
        }

        // Обработчик кнопки сохранить
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbReaders.SelectedValue == null || cbBooks.SelectedValue == null || dpReturnDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите читателя, книгу и дату возврата.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int readerId = (int)cbReaders.SelectedValue;
            int bookId = (int)cbBooks.SelectedValue;
            DateTime expectedReturnDate = dpReturnDate.SelectedDate.Value;

            if (expectedReturnDate < DateTime.Today)
            {
                MessageBox.Show("Дата возврата не может быть меньше текущей даты.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на 18+
            if (!CanIssueBook(bookId, readerId, out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Возрастное ограничение",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // Выдача книги
            try
            {
                string insertQuery = @"
                    INSERT INTO BorrowedBooks (BookId, ReaderId, BorrowDate, ExpectedReturnDate, IssuedBy) 
                    VALUES (@BookId, @ReaderId, @BorrowDate, @ExpectedReturnDate, @IssuedBy)";

                DbHelper.Execute(insertQuery,
                    new SqlParameter("@BookId", bookId),
                    new SqlParameter("@ReaderId", readerId),
                    new SqlParameter("@BorrowDate", DateTime.Now),
                    new SqlParameter("@ExpectedReturnDate", expectedReturnDate),
                    new SqlParameter("@IssuedBy", _staffId));

                DbHelper.Execute("UPDATE Books SET IsAvailable = 0 WHERE Id = @Id",
                    new SqlParameter("@Id", bookId));

                MessageBox.Show("Книга успешно выдана.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}