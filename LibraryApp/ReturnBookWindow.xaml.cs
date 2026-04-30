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
    public partial class ReturnBookWindow : Window
    {
        public event Action BookReturned;

        private DataTable borrowsTable;

        public ReturnBookWindow()
        {
            InitializeComponent();
            LoadBorrows();
        }

        private void LoadBorrows()
        {
            string query = @"
                SELECT bb.Id, b.Title, r.FullName, bb.BorrowDate, bb.ExpectedReturnDate
                FROM BorrowedBooks bb
                JOIN Books b ON bb.BookId = b.Id
                JOIN Readers r ON bb.ReaderId = r.Id
                WHERE bb.ReturnDate IS NULL";

            borrowsTable = DbHelper.GetData(query);

            if (borrowsTable.Rows.Count == 0)
            {
                spSearch.Visibility = Visibility.Collapsed;
                tbNoBorrows.Visibility = Visibility.Visible;
                btnReturn.IsEnabled = false;
                return;
            }

            borrowsTable.Columns.Add("DisplayText", typeof(string));

            foreach (DataRow row in borrowsTable.Rows)
            {
                DateTime borrowDate = Convert.ToDateTime(row["BorrowDate"]);
                DateTime expectedReturn = Convert.ToDateTime(row["ExpectedReturnDate"]);
                string title = row["Title"].ToString();
                string fullName = row["FullName"].ToString();

                string overdue = (expectedReturn < DateTime.Now) ? " (просрочена)" : "";
                row["DisplayText"] = fullName + " - " + title + " (взято: " + borrowDate.ToShortDateString() + ")" + overdue;
            }

            borrowsTable.DefaultView.RowFilter = string.Empty;
            cbBorrows.ItemsSource = borrowsTable.DefaultView;
            cbBorrows.SelectedIndex = -1;
            cbBorrows.Text = string.Empty;

            spSearch.Visibility = Visibility.Visible;
            tbNoBorrows.Visibility = Visibility.Collapsed;
            btnReturn.IsEnabled = true;
        }

        // Простая фильтрация + фикс курсора
        private void CbBorrows_KeyUp(object sender, KeyEventArgs e)
        {
            if (borrowsTable == null) return;

            string filterText = cbBorrows.Text;

            if (string.IsNullOrEmpty(filterText))
            {
                borrowsTable.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                string safeFilter = filterText.Replace("'", "''");
                borrowsTable.DefaultView.RowFilter = "DisplayText LIKE '%" + safeFilter + "%'";
            }

            if (!string.IsNullOrEmpty(filterText))
            {
                cbBorrows.IsDropDownOpen = true;

                // Фикс курсора — чтобы текст не выделялся
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    var textBox = cbBorrows.Template.FindName("PART_EditableTextBox", cbBorrows) as TextBox;
                    if (textBox != null)
                    {
                        textBox.CaretIndex = textBox.Text.Length;
                        textBox.Select(textBox.Text.Length, 0);
                    }
                }));
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (cbBorrows.SelectedValue == null)
            {
                MessageBox.Show("Выберите книгу из списка.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int borrowId = (int)cbBorrows.SelectedValue;

            string updateBorrow = "UPDATE BorrowedBooks SET ReturnDate=@ReturnDate WHERE Id=@Id";
            DbHelper.Execute(updateBorrow,
                new SqlParameter("@ReturnDate", DateTime.Now),
                new SqlParameter("@Id", borrowId));

            string bookIdQuery = "SELECT BookId FROM BorrowedBooks WHERE Id=@Id";
            DataTable dt = DbHelper.GetData(bookIdQuery, new SqlParameter("@Id", borrowId));
            int bookId = (int)dt.Rows[0]["BookId"];

            string updateBook = "UPDATE Books SET IsAvailable=1 WHERE Id=@Id";
            DbHelper.Execute(updateBook, new SqlParameter("@Id", bookId));

            MessageBox.Show("Книга успешно возвращена.");

            LoadBorrows();
            BookReturned?.Invoke();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}