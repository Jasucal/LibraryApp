using LibraryApp.DataAccess;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LibraryApp
{
    public partial class UserMainWindow : Window
    {
        private int _userId;
        private bool _isUserAdult = true;

        public UserMainWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;

            CheckUserAge();

            LoadAllData();

            txtSearch.TextChanged += TxtSearch_TextChanged;
            tabMain.SelectionChanged += TabMain_SelectionChanged;
        }

        private void CheckUserAge()
        {
            string query = "SELECT DateOfBirth FROM Readers WHERE Id = @Id";
            var result = DbHelper.GetData(query, new SqlParameter("@Id", _userId));

            if (result.Rows.Count > 0 && result.Rows[0]["DateOfBirth"] != DBNull.Value)
            {
                DateTime dob = Convert.ToDateTime(result.Rows[0]["DateOfBirth"]);
                int age = DateTime.Now.Year - dob.Year;
                if (DateTime.Now.DayOfYear < dob.DayOfYear) age--;
                _isUserAdult = (age >= 18);
            }
        }

        // Загрузка всех данных
        private void LoadAllData()
        {
            LoadBooksGrid();
            LoadBorrowedGrid();
        }

        // Загрузка каталога книг с учётом возраста
        private void LoadBooksGrid(string search = "")
        {
            string query;

            if (_isUserAdult)
            {
                query = string.IsNullOrWhiteSpace(search)
                    ? Queries.UserBooks_All
                    : Queries.UserBooks_WithSearch;
            }
            else
            {
                query = string.IsNullOrWhiteSpace(search)
                    ? Queries.UserBooks_All_Minor
                    : Queries.UserBooks_WithSearch_Minor;
            }

            SqlParameter[] parameters = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                parameters = new SqlParameter[] { new SqlParameter("@search", "%" + search + "%") };
            }

            dgBooks.ItemsSource = DbHelper.GetData(query, parameters).DefaultView;
        }

        // Загрузка моих выданных книг
        private void LoadBorrowedGrid(string search = "")
        {
            string query = string.IsNullOrWhiteSpace(search)
                ? Queries.UserBorrowed_Mine
                : Queries.UserBorrowed_MineWithSearch;

            if (string.IsNullOrWhiteSpace(search))
            {
                var param = new SqlParameter("@userId", _userId);
                dgBorrowed.ItemsSource = DbHelper.GetData(query, param).DefaultView;
            }
            else
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", _userId),
                    new SqlParameter("@search", "%" + search + "%")
                };
                dgBorrowed.ItemsSource = DbHelper.GetData(query, parameters).DefaultView;
            }
        }

        // Поиск
        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            // Определяем, какая вкладка активна, и фильтруем только её
            TabItem selectedTab = tabMain.SelectedItem as TabItem;
            if (selectedTab == null) return;

            string header = selectedTab.Header.ToString();

            if (header == "Все книги")
            {
                LoadBooksGrid(keyword);
            }
            else if (header == "Мои книги")
            {
                LoadBorrowedGrid(keyword);
            }
        }

        // Переключени Вкладок
        private void TabMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is System.Windows.Controls.TabControl)
            {
                txtSearch.Text = string.Empty;

                TabItem selectedTab = tabMain.SelectedItem as TabItem;
                if (selectedTab != null)
                {
                    string header = selectedTab.Header.ToString();
                    if (header == "Все книги")
                    {
                        LoadBooksGrid();
                    }
                    else if (header == "Мои книги")
                    {
                        LoadBorrowedGrid();
                    }
                }
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

        // Выйти
        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            var profile = new ProfileWindow(_userId);
            profile.Owner = this;
            profile.ShowDialog();
        }
    }
}