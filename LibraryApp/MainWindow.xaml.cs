using LibraryApp.DataAccess;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LibraryApp
{
    public partial class MainWindow : Window
    {
        private int _currentUserId;
        
        public MainWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadAllData();
        }

        // Загрузка книг
        private void LoadBooks()
        {
            dgBooks.ItemsSource = DbHelper.GetData(Queries.Books_All).DefaultView;
        }

        // Загрузка записей о выдаче
        private void LoadBorrowed()
        {
            dgBorrowed.ItemsSource = DbHelper.GetData(Queries.Borrowed_Active).DefaultView;
        }

        // Загрузка авторов
        private void LoadAuthors()
        {
            dgAuthors.ItemsSource = DbHelper.GetData(Queries.Authors_All).DefaultView;
        }

        // Загрузка читателей
        private void LoadReaders()
        {
            dgReaders.ItemsSource = DbHelper.GetData(Queries.Readers_All).DefaultView;
        }

        // Загрузка истории
        private void LoadHistory()
        {
            dgHistory.ItemsSource = DbHelper.GetData(Queries.History_All).DefaultView;
        }

        // Загрузка статистики
        private void LoadPopularStats()
        {
            dgPopularBooks.ItemsSource = DbHelper.GetData(Queries.Stats_PopularBooks).DefaultView;
            dgPopularAuthors.ItemsSource = DbHelper.GetData(Queries.Stats_PopularAuthors).DefaultView;
        }

        // Полная загрузка данных
        private void LoadAllData()
        {
            LoadBooks();
            LoadBorrowed();
            LoadAuthors();
            LoadReaders();
            LoadHistory();
            LoadPopularStats();
        }

        // Поиск
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                LoadAllData();
                return;
            }

            TabItem selectedTab = tabMain.SelectedItem as TabItem;
            if (selectedTab == null) return;

            string header = selectedTab.Header.ToString();
            SqlParameter param = new SqlParameter("@keyword", "%" + keyword + "%");

            if (header == "Книги")
            {
                dgBooks.ItemsSource = DbHelper.GetData(Queries.Books_WithSearch, param)?.DefaultView;
                dgBorrowed.ItemsSource = DbHelper.GetData(Queries.Borrowed_WithSearch, param)?.DefaultView;
            }
            else if (header == "История")
            {
                dgHistory.ItemsSource = DbHelper.GetData(Queries.History_WithSearch, param)?.DefaultView;
            }
            else if (header == "Авторы")
            {
                dgAuthors.ItemsSource = DbHelper.GetData(Queries.Authors_WithSearch, param)?.DefaultView;
            }
            else if (header == "Пользователи")
            {
                dgReaders.ItemsSource = DbHelper.GetData(Queries.Readers_WithSearch, param)?.DefaultView;
            }
        }

        // Добавить книгу
        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditBookWindow();
            if (window.ShowDialog() == true)
            {
                LoadBooks();
                LoadPopularStats();
            }
        }

        // Редактировать книгу
        private void BtnEditBook_Click(object sender, RoutedEventArgs e)
        {
            var row = dgBooks.SelectedItem as DataRowView;
            if (row == null) return;

            int bookId = (int)row["Id"];
            var window = new AddEditBookWindow(bookId);
            if (window.ShowDialog() == true)
            {
                LoadBooks();
                LoadPopularStats();
            }
        }

        // Удалить книгу
        private void BtnDeleteBook_Click(object sender, RoutedEventArgs e)
        {
            var row = dgBooks.SelectedItem as DataRowView;
            if (row == null) return;

            int bookId = (int)row["Id"];

            if (MessageBox.Show("Удалить книгу?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                var result = DbHelper.GetData(Queries.Books_CheckBeforeDelete, new SqlParameter("@Id", bookId));
                if (result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Нельзя удалить книгу, которая была выдана или выдавалась раньше, читателю!", "Ошибка удаления",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DbHelper.Execute(Queries.Books_Delete, new SqlParameter("@Id", bookId));
                LoadBooks();
                LoadPopularStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении книги: " + ex.Message);
            }
        }

        // Выдача книги
        private void BtnBorrowBook_Click(object sender, RoutedEventArgs e)
        {
            var window = new BorrowBookWindow(_currentUserId);
            if (window.ShowDialog() == true)
            {
                LoadBooks();
                LoadBorrowed();
                LoadPopularStats();
            }
        }

        // Возврат книги
        private void BtnReturnBook_Click(object sender, RoutedEventArgs e)
        {
            // Передаём ID текущего сотрудника
            var returnWindow = new ReturnBookWindow(_currentUserId);
            returnWindow.BookReturned += OnBookReturned;
            returnWindow.Show();
        }

        private void OnBookReturned()
        {
            LoadBooks();
            LoadBorrowed();
            LoadHistory();
            LoadPopularStats();
        }

        // Добавить автора
        private void BtnAddAuthor_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditAuthorWindow();
            if (window.ShowDialog() == true) LoadAuthors();
        }

        // Редактировать автора
        private void BtnEditAuthor_Click(object sender, RoutedEventArgs e)
        {
            var row = dgAuthors.SelectedItem as DataRowView;
            if (row == null) return;

            int authorId = (int)row["Id"];
            var window = new AddEditAuthorWindow(authorId);
            if (window.ShowDialog() == true) LoadAuthors();
        }

        // Удалить автора
        private void BtnDeleteAuthor_Click(object sender, RoutedEventArgs e)
        {
            var row = dgAuthors.SelectedItem as DataRowView;
            if (row == null) return;

            int authorId = (int)row["Id"];

            if (MessageBox.Show("Удалить автора?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                var result = DbHelper.GetData(Queries.Authors_CheckBeforeDelete, new SqlParameter("@AuthorId", authorId));
                if (result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Нельзя удалить автора, у которого есть книги!", "Ошибка удаления",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DbHelper.Execute(Queries.Authors_Delete, new SqlParameter("@Id", authorId));
                LoadAuthors();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении автора: " + ex.Message);
            }
        }

        // Добавить читателя
        private void BtnAddReader_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditReaderWindow();
            if (window.ShowDialog() == true) LoadReaders();
        }

        // Редактировать читателя
        private void BtnEditReader_Click(object sender, RoutedEventArgs e)
        {
            var row = dgReaders.SelectedItem as DataRowView;
            if (row == null) return;

            int readerId = (int)row["Id"];
            var window = new AddEditReaderWindow(readerId);
            if (window.ShowDialog() == true) LoadReaders();
        }

        // Удалить читателя
        private void BtnDeleteReader_Click(object sender, RoutedEventArgs e)
        {
            var row = dgReaders.SelectedItem as DataRowView;
            if (row == null) return;

            int readerId = (int)row["Id"];

            if (MessageBox.Show("Удалить читателя?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                var result = DbHelper.GetData(Queries.Readers_CheckBeforeDelete, new SqlParameter("@ReaderId", readerId));
                if (result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Нельзя удалить читателя, пока у него есть невозвращённые книги!", "Ошибка удаления",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DbHelper.Execute(Queries.Readers_Delete, new SqlParameter("@Id", readerId));
                LoadReaders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении читателя: " + ex.Message);
            }
        }

        // Отчёты
        private void BtnExportBooks_Click(object sender, RoutedEventArgs e) => Reports.GenerateBooksReport();

        private void BtnExportAuthors_Click(object sender, RoutedEventArgs e) => Reports.GenerateAuthorsReport();

        private void BtnExportReaders_Click(object sender, RoutedEventArgs e) => Reports.GenerateReadersReport();

        private void BtnExportHistory_Click(object sender, RoutedEventArgs e) => Reports.GenerateHistoryReport();

        private void BtnExportStats_Click(object sender, RoutedEventArgs e)
        {
            var booksView = dgPopularBooks.ItemsSource as DataView;
            var authorsView = dgPopularAuthors.ItemsSource as DataView;
            Reports.GenerateStatsReport(booksView?.ToTable(), authorsView?.ToTable());
        }

        // Информация о читателе по двойному клику на выдачу
        private void dgBorrowed_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = dgBorrowed.SelectedItem as DataRowView;
            if (row == null) return;

            string fullName = row["ReaderName"].ToString();
            DateTime? dob = null;
            string phone = "", email = "";

            var dt = DbHelper.GetData(Queries.Readers_GetInfo, new SqlParameter("@fullName", fullName));
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["DateOfBirth"] != DBNull.Value)
                    dob = Convert.ToDateTime(dt.Rows[0]["DateOfBirth"]);
                phone = dt.Rows[0]["PhoneNumber"].ToString();
                email = dt.Rows[0]["Email"].ToString();
            }

            var infoWindow = new ReaderInfoWindow(fullName, dob, phone, email);
            infoWindow.ShowDialog();
        }

        // Переключение вкладок
        private void TabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtSearch.Text = string.Empty;

            TabItem selectedTab = tabMain.SelectedItem as TabItem;
            if (selectedTab != null && selectedTab.Header.ToString() == "Статистика")
            {
                LoadPopularStats();
            }
        }

        // Сброс сортировки при клике на фон DataGrid
        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            var source = e.OriginalSource as DependencyObject;

            while (source != null && !(source is DataGrid))
            {
                if (source is DataGridRow || source is DataGridCell || source is DataGridColumnHeader)
                    return;
                source = VisualTreeHelper.GetParent(source);
            }

            foreach (var column in dataGrid.Columns)
                column.SortDirection = null;
        }

        // Выход
        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            var profile = new ProfileWindow(_currentUserId); // или _userId в UserMainWindow
            profile.Owner = this; //  Указываем главное окно как владельца
            profile.ShowDialog();
        }
    }
}