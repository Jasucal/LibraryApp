using LibraryApp.DataAccess;
using System;
using System.Data.SqlClient;
using System.Windows;

namespace LibraryApp
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow(int userId)
        {
            InitializeComponent();
            LoadProfile(userId);
        }

        private void LoadProfile(int userId)
        {
            // Загружаем основную информацию и роль
            string infoQuery = "SELECT FullName, DateOfBirth, PhoneNumber, Email, Role FROM Readers WHERE Id = @Id";
            var infoDt = DbHelper.GetData(infoQuery, new SqlParameter("@Id", userId));

            if (infoDt.Rows.Count == 0)
            {
                MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            var row = infoDt.Rows[0];
            tbFullName.Text = row["FullName"].ToString();
            tbDob.Text = Convert.ToDateTime(row["DateOfBirth"]).ToString("dd.MM.yyyy");
            tbPhone.Text = row["PhoneNumber"].ToString();
            tbEmail.Text = row["Email"].ToString();

            int role = Convert.ToInt32(row["Role"]);
            tbRole.Text = role == 1 ? "👨‍💼 Библиотекарь" : "👤 Читатель";

            // Показываем статистику в зависимости от роли
            if (role == 1)
            {
                gbLibrarian.Visibility = Visibility.Visible;
                LoadLibrarianStats(userId);
            }
            else
            {
                gbReader.Visibility = Visibility.Visible;
                LoadReaderStats(userId);
            }
        }

        private void LoadLibrarianStats(int userId)
        {
            string query = @"
                SELECT
                  (SELECT COUNT(*) FROM BorrowedBooks WHERE IssuedBy = @Id) AS BooksIssued,
                  (SELECT COUNT(*) FROM BorrowedBooks WHERE ReturnedBy = @Id) AS BooksReturned";

            var dt = DbHelper.GetData(query, new SqlParameter("@Id", userId));
            if (dt.Rows.Count > 0)
            {
                tbIssued.Text = dt.Rows[0]["BooksIssued"].ToString();
                tbReturned.Text = dt.Rows[0]["BooksReturned"].ToString();
            }
        }

        private void LoadReaderStats(int userId)
        {
            //  Всего прочитано (успешно возвращённые книги)
            string readQuery = @"
                SELECT COUNT(*) AS TotalRead 
                FROM BorrowedBooks 
                WHERE ReaderId = @Id AND ReturnDate IS NOT NULL";

            var readDt = DbHelper.GetData(readQuery, new SqlParameter("@Id", userId));
            tbRead.Text = readDt.Rows.Count > 0 ? readDt.Rows[0]["TotalRead"].ToString() : "0";

            // Любимая книга (самая частая выдача)
            string favQuery = @"
                SELECT TOP 1 b.Title
                FROM BorrowedBooks bb
                JOIN Books b ON bb.BookId = b.Id
                WHERE bb.ReaderId = @Id AND bb.ReturnDate IS NOT NULL
                GROUP BY b.Title
                ORDER BY COUNT(bb.BookId) DESC";

            var favDt = DbHelper.GetData(favQuery, new SqlParameter("@Id", userId));
            tbFavorite.Text = favDt.Rows.Count > 0 ? favDt.Rows[0]["Title"].ToString() : "Нет данных";
        }

        //  Выход из аккаунта с подтверждением
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы действительно хотите выйти из профиля?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();

                Application.Current.MainWindow = loginWindow;

                loginWindow.Show();

                this.Owner?.Close();
                this.Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}