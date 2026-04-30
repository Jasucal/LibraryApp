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

        public BorrowBookWindow()
        {
            InitializeComponent();
            LoadReaders();
            LoadBooks();
            dpReturnDate.SelectedDate = DateTime.Now.AddDays(14);
        }

        private void LoadReaders()
        {
            string query = @"
                SELECT 
                    Id, 
                    FullName, 
                    FullName + ' (' + CONVERT(varchar, DateOfBirth, 104) + ')' AS DisplayName
                FROM Readers
                WHERE Role <> '1'";

            readersTable = DbHelper.GetData(query);

            cbReaders.ItemsSource = readersTable.DefaultView;
            cbReaders.DisplayMemberPath = "FullName";
            cbReaders.SelectedValuePath = "Id";
            cbReaders.SelectedIndex = -1;
            cbReaders.Text = string.Empty;
        }

        private void LoadBooks()
        {
            string query = @"
                SELECT 
                    b.Id, 
                    b.Title + ' (' + a.Name + ', ' + b.Publisher + ')' AS DisplayTitle
                FROM Books b
                INNER JOIN Authors a ON b.AuthorId = a.Id
                WHERE b.IsAvailable = 1";

            booksTable = DbHelper.GetData(query);

            cbBooks.ItemsSource = booksTable.DefaultView;
            cbBooks.DisplayMemberPath = "DisplayTitle";
            cbBooks.SelectedValuePath = "Id";
            cbBooks.SelectedIndex = -1;
            cbBooks.Text = string.Empty;
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

                // Фикс курсора чтобы текст не выделялся
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

                // Фикс курсора чтобы текст не выделялся
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

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbReaders.SelectedValue == null || cbBooks.SelectedValue == null || dpReturnDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите читателя, книгу и дату возврата.");
                return;
            }

            int readerId = (int)cbReaders.SelectedValue;
            int bookId = (int)cbBooks.SelectedValue;
            DateTime expectedReturnDate = dpReturnDate.SelectedDate.Value;

            if (expectedReturnDate < DateTime.Today)
            {
                MessageBox.Show("Дата возврата не может быть меньше текущей даты.");
                return;
            }

            string insertQuery = @"
                INSERT INTO BorrowedBooks (BookId, ReaderId, BorrowDate, ExpectedReturnDate) 
                VALUES (@BookId, @ReaderId, @BorrowDate, @ExpectedReturnDate)";

            SqlParameter[] insertParams = new SqlParameter[]
            {
                new SqlParameter("@BookId", bookId),
                new SqlParameter("@ReaderId", readerId),
                new SqlParameter("@BorrowDate", DateTime.Now),
                new SqlParameter("@ExpectedReturnDate", expectedReturnDate)
            };

            DbHelper.Execute(insertQuery, insertParams);

            string updateQuery = "UPDATE Books SET IsAvailable = 0 WHERE Id = @Id";
            SqlParameter[] updateParams = new SqlParameter[]
            {
                new SqlParameter("@Id", bookId)
            };
            DbHelper.Execute(updateQuery, updateParams);

            MessageBox.Show("Книга успешно выдана.");
            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}