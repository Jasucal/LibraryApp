using LibraryApp.DataAccess;
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

        public UserMainWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;

            // Загружаем все данные при старте
            LoadAllData();

            // Привязываем события
            txtSearch.TextChanged += TxtSearch_TextChanged;
            tabMain.SelectionChanged += TabMain_SelectionChanged;
        }

        // Загрузка всех данных
        private void LoadAllData()
        {
            LoadBooksGrid();
            LoadBorrowedGrid();
        }

        // Загрузка каталога книг
        private void LoadBooksGrid(string search = "")
        {
            string query = string.IsNullOrWhiteSpace(search)
                ? Queries.UserBooks_All
                : Queries.UserBooks_WithSearch;

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
                // Только ID пользователя
                var param = new SqlParameter("@userId", _userId);
                dgBorrowed.ItemsSource = DbHelper.GetData(query, param).DefaultView;
            }
            else
            {
                // ID пользователя + поиск
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", _userId),
                    new SqlParameter("@search", "%" + search + "%")
                };
                dgBorrowed.ItemsSource = DbHelper.GetData(query, parameters).DefaultView;
            }
        }

        // Обработка поиска
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

        // Очистка поиска при переключении вкладки
        private void TabMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is System.Windows.Controls.TabControl)
            {
                txtSearch.Text = string.Empty;
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

        // Выход из аккаунта
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}