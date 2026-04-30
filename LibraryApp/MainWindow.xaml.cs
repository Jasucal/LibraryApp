using LibraryApp.DataAccess;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LibraryApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
        // Загрузка читателй
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
                LoadBooks();        // Обновляем только книги
                LoadPopularStats(); // И статистику
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
                    MessageBox.Show("Нельзя удалить книгу, которая была выдана читателю!", "Ошибка удаления",
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
            var window = new BorrowBookWindow();
            if (window.ShowDialog() == true)
            {
                LoadBooks();        // Обновить статус доступности
                LoadBorrowed();     // Показать новую выдачу
                LoadPopularStats(); // Обновить статистику
            }
        }

        // Возврат книги
        private void BtnReturnBook_Click(object sender, RoutedEventArgs e)
        {
            var returnWindow = new ReturnBookWindow();
            returnWindow.BookReturned += OnBookReturned;
            returnWindow.Show();
        }

        // Возврат книги
        private void OnBookReturned()
        {
            LoadBooks();        // Книга снова доступна
            LoadBorrowed();     // Убрать из активных выдач
            LoadHistory();      // Показать в истории
            LoadPopularStats(); // Обновить статистику
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

        // Выход
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        // Модуль отчётов
        private void CreateReport(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new ReportsWindow();
            reportsWindow.ShowDialog();
        }

        // Проверка информации читателя по выданной книге
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
        // Очистка выделения 
        private void ClearDataGrid(DataGrid dg)
        {
            if (dg == null) return;

            dg.SelectedItem = null;
            dg.CurrentCell = new DataGridCellInfo();
            foreach (var column in dg.Columns)
                column.SortDirection = null;
            Keyboard.ClearFocus();
        }

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            var originalSource = e.OriginalSource as DependencyObject;
            bool clickedOnColumnHeader = false;
            bool clickedOnDataRow = false;

            while (originalSource != null)
            {
                if (originalSource is DataGridColumnHeader) { clickedOnColumnHeader = true; break; }
                if (originalSource is DataGridRow || originalSource is DataGridCell) clickedOnDataRow = true;
                if (originalSource is DataGrid) break;
                originalSource = VisualTreeHelper.GetParent(originalSource);
            }

            if (!clickedOnColumnHeader && !clickedOnDataRow)
            {
                ClearDataGrid(dataGrid);
                e.Handled = true;
            }
        }
        // Отчёт о статистике
        private void BtnExportStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("LibraryApp");

                DataView booksView = dgPopularBooks.ItemsSource as DataView;
                DataView authorsView = dgPopularAuthors.ItemsSource as DataView;

                DataTable books = booksView?.ToTable();
                DataTable authors = authorsView?.ToTable();

                if ((books == null || books.Rows.Count == 0) && (authors == null || authors.Rows.Count == 0))
                {
                    MessageBox.Show("Нет данных для отчёта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var excel = new ExcelPackage())
                {
                    var ws = excel.Workbook.Worksheets.Add("Статистика");

                    int row = 1;
                    ws.Cells[row, 1].Value = "ПОПУЛЯРНЫЕ КНИГИ";
                    ws.Cells[row, 1, row, 5].Merge = true;
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    row++;

                    ws.Cells[row, 1].Value = "Место";
                    ws.Cells[row, 2].Value = "Название";
                    ws.Cells[row, 3].Value = "Автор";
                    ws.Cells[row, 4].Value = "Выдач";
                    ws.Cells[row, 5].Value = "Жанр";

                    using (var range = ws.Cells[row, 1, row, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    row++;

                    if (books != null)
                    {
                        foreach (DataRow book in books.Rows)
                        {
                            ws.Cells[row, 1].Value = book["Rank"];
                            ws.Cells[row, 2].Value = book["Title"];
                            ws.Cells[row, 3].Value = book["AuthorName"];
                            ws.Cells[row, 4].Value = book["BorrowCount"];
                            ws.Cells[row, 5].Value = book["Genre"];
                            row++;
                        }
                    }

                    row++;

                    ws.Cells[row, 1].Value = "ПОПУЛЯРНЫЕ АВТОРЫ";
                    ws.Cells[row, 1, row, 5].Merge = true;
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    row++;

                    ws.Cells[row, 1].Value = "Место";
                    ws.Cells[row, 2].Value = "ФИО";
                    ws.Cells[row, 3].Value = "Книг в выдаче";
                    ws.Cells[row, 4].Value = "Уникальных книг";
                    ws.Cells[row, 5].Value = "Дата рождения";

                    using (var range = ws.Cells[row, 1, row, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    row++;

                    if (authors != null)
                    {
                        foreach (DataRow author in authors.Rows)
                        {
                            ws.Cells[row, 1].Value = author["Rank"];
                            ws.Cells[row, 2].Value = author["Name"];
                            ws.Cells[row, 3].Value = author["TotalBorrows"];
                            ws.Cells[row, 4].Value = author["UniqueBooks"];
                            ws.Cells[row, 5].Value = author["DateOfBirth"] != DBNull.Value
                                ? Convert.ToDateTime(author["DateOfBirth"]).ToString("dd.MM.yyyy")
                                : "";
                            row++;
                        }
                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    string fileName = "LibraryStats_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                    if (File.Exists(path)) File.Delete(path);
                    File.WriteAllBytes(path, excel.GetAsByteArray());

                    MessageBox.Show("Отчёт по статистике создан:\n" + path, "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}