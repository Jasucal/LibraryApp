using LibraryApp.DataAccess;
using OfficeOpenXml;
using System;
using System.Data;
using System.IO;
using System.Windows;


namespace LibraryApp
{
    public partial class ReportsWindow : Window
    {
        public ReportsWindow()
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialPersonal("LibraryApp");
        }

        // Отчёт для книг
        private void BtnBooksReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("LibraryApp");

                string query = @"
                    SELECT b.Id, b.Title, a.Name AS AuthorName, b.Publisher, b.Genre, b.Year, b.IsAvailable
                    FROM Books b
                    JOIN Authors a ON b.AuthorId = a.Id";

                DataTable books = DbHelper.GetData(query);
                if (books.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для отчёта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var excel = new ExcelPackage())
                {
                    var ws = excel.Workbook.Worksheets.Add("Книги");

                    ws.Cells[1, 1].Value = "ID";
                    ws.Cells[1, 2].Value = "Название";
                    ws.Cells[1, 3].Value = "Автор";
                    ws.Cells[1, 4].Value = "Издательство";
                    ws.Cells[1, 5].Value = "Жанр";
                    ws.Cells[1, 6].Value = "Год";
                    ws.Cells[1, 7].Value = "Доступна";

                    using (var range = ws.Cells[1, 1, 1, 7])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    int row = 2;
                    foreach (DataRow book in books.Rows)
                    {
                        ws.Cells[row, 1].Value = book["Id"];
                        ws.Cells[row, 2].Value = book["Title"];
                        ws.Cells[row, 3].Value = book["AuthorName"];
                        ws.Cells[row, 4].Value = book["Publisher"];
                        ws.Cells[row, 5].Value = book["Genre"];
                        ws.Cells[row, 6].Value = book["Year"];
                        ws.Cells[row, 7].Value = (bool)book["IsAvailable"] ? "Да" : "Нет";
                        row++;
                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BooksReport.xlsx");
                    if (File.Exists(path)) File.Delete(path);
                    File.WriteAllBytes(path, excel.GetAsByteArray());

                    MessageBox.Show("Отчёт по книгам создан:\n" + path, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Отчёт для авторов
        private void BtnAuthorsReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("LibraryApp");

                string query = @"SELECT Id, Name, DateOfBirth FROM Authors";
                DataTable authors = DbHelper.GetData(query);
                if (authors.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для отчёта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var excel = new ExcelPackage())
                {
                    var ws = excel.Workbook.Worksheets.Add("Авторы");

                    ws.Cells[1, 1].Value = "ID";
                    ws.Cells[1, 2].Value = "ФИО";
                    ws.Cells[1, 3].Value = "Дата рождения";

                    using (var range = ws.Cells[1, 1, 1, 3])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    int row = 2;
                    foreach (DataRow author in authors.Rows)
                    {
                        ws.Cells[row, 1].Value = author["Id"];
                        ws.Cells[row, 2].Value = author["Name"];

                        object dob = author["DateOfBirth"];
                        ws.Cells[row, 3].Value = (dob != DBNull.Value) ? Convert.ToDateTime(dob).ToString("dd.MM.yyyy") : "";

                        row++;
                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AuthorsReport.xlsx");
                    if (File.Exists(path)) File.Delete(path);
                    File.WriteAllBytes(path, excel.GetAsByteArray());

                    MessageBox.Show("Отчёт по авторам создан:\n" + path, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Отчёт для читателей
        private void BtnReadersReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("LibraryApp");

                // Выбираем только обычных пользователей
                string query = @"
                    SELECT FullName, DateOfBirth, PhoneNumber, Email
                    FROM Readers
                    WHERE Role <> 1";

                DataTable readers = DbHelper.GetData(query);
                if (readers.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для отчёта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var excel = new ExcelPackage())
                {
                    var ws = excel.Workbook.Worksheets.Add("Читатели");

                    ws.Cells[1, 1].Value = "ФИО";
                    ws.Cells[1, 2].Value = "Дата рождения";
                    ws.Cells[1, 3].Value = "Телефон";
                    ws.Cells[1, 4].Value = "Email";

                    using (var range = ws.Cells[1, 1, 1, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    int row = 2;
                    foreach (DataRow reader in readers.Rows)
                    {
                        ws.Cells[row, 1].Value = reader["FullName"];

                        object dob = reader["DateOfBirth"];
                        ws.Cells[row, 2].Value = (dob != DBNull.Value) ? Convert.ToDateTime(dob).ToString("dd.MM.yyyy") : "";

                        ws.Cells[row, 3].Value = reader["PhoneNumber"];
                        ws.Cells[row, 4].Value = reader["Email"];
                        row++;
                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReadersReport.xlsx");
                    if (File.Exists(path)) File.Delete(path);
                    File.WriteAllBytes(path, excel.GetAsByteArray());

                    MessageBox.Show("Отчёт по читателям создан:\n" + path, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Отчёт по истории
        private void BtnHistoryReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("LibraryApp");

                string query = @"
                    SELECT b.Title AS BookTitle,
                        r.FullName AS ReaderName,
                        bb.BorrowDate,
                        bb.ExpectedReturnDate,
                        bb.ReturnDate
                    FROM BorrowedBooks bb
                    JOIN Books b ON bb.BookId = b.Id
                    JOIN Readers r ON bb.ReaderId = r.Id
                    WHERE bb.ReturnDate IS NOT NULL
                    ORDER BY bb.ReturnDate DESC";

                DataTable history = DbHelper.GetData(query);
                if (history.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для отчёта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var excel = new ExcelPackage())
                {
                    var ws = excel.Workbook.Worksheets.Add("История");

                    ws.Cells[1, 1].Value = "Книга";
                    ws.Cells[1, 2].Value = "Читатель";
                    ws.Cells[1, 3].Value = "Дата выдачи";
                    ws.Cells[1, 4].Value = "Плановая дата возврата";
                    ws.Cells[1, 5].Value = "Фактическая дата возврата";

                    using (var range = ws.Cells[1, 1, 1, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    int row = 2;
                    foreach (DataRow record in history.Rows)
                    {
                        ws.Cells[row, 1].Value = record["BookTitle"];
                        ws.Cells[row, 2].Value = record["ReaderName"];

                        object borrow = record["BorrowDate"];
                        ws.Cells[row, 3].Value = (borrow != DBNull.Value) ? Convert.ToDateTime(borrow).ToString("dd.MM.yyyy") : "";

                        object expected = record["ExpectedReturnDate"];
                        ws.Cells[row, 4].Value = (expected != DBNull.Value) ? Convert.ToDateTime(expected).ToString("dd.MM.yyyy") : "";

                        object returned = record["ReturnDate"];
                        ws.Cells[row, 5].Value = (returned != DBNull.Value) ? Convert.ToDateTime(returned).ToString("dd.MM.yyyy") : "";

                        row++;
                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoryReport.xlsx");
                    if (File.Exists(path)) File.Delete(path);
                    File.WriteAllBytes(path, excel.GetAsByteArray());

                    MessageBox.Show("Отчёт по истории создан:\n" + path, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}