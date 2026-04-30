using System;
using System.Data;
using System.Windows;
using LibraryApp.DataAccess;
using System.Data.SqlClient;

namespace LibraryApp
{
    public partial class AddEditAuthorWindow : Window
    {
        private int _authorId = 0;

        public AddEditAuthorWindow(int authorId = 0)
        {
            InitializeComponent();
            _authorId = authorId;

            if (_authorId > 0)
                LoadAuthorData();
        }

        private void LoadAuthorData()
        {
            string query = "SELECT * FROM Authors WHERE Id = @Id";
            DataTable dt = DbHelper.GetData(query,
                new SqlParameter("@Id", _authorId));

            if (dt.Rows.Count > 0)
            {
                txtName.Text = dt.Rows[0]["Name"].ToString();

                if (dt.Rows[0]["DateOfBirth"] != DBNull.Value)
                    dpDateOfBirth.SelectedDate = Convert.ToDateTime(dt.Rows[0]["DateOfBirth"]);
            }
        }

        // Обработка кнопки "Сохранить"
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            DateTime? dateOfBirth = dpDateOfBirth.SelectedDate;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите имя автора.");
                return;
            }

            // Проверка даты рождения
            if (dateOfBirth.HasValue && dateOfBirth.Value > DateTime.Today)
            {
                MessageBox.Show("Дата рождения не может быть больше текущей даты.");
                return;
            }

            // Проверка автора
            string checkQuery = "SELECT * FROM Authors WHERE Name = @Name" + (_authorId > 0 ? " AND Id <> @Id" : "");
            var parameters = _authorId > 0
                ? new SqlParameter[] { new SqlParameter("@Name", name), new SqlParameter("@Id", _authorId) }
                : new SqlParameter[] { new SqlParameter("@Name", name) };

            DataTable existing = DbHelper.GetData(checkQuery, parameters);

            if (existing.Rows.Count > 0)
            {
                MessageBox.Show("Автор с таким именем уже существует.");
                return;
            }

            object dobValue = dateOfBirth.HasValue ? (object)dateOfBirth.Value : DBNull.Value;

            if (_authorId == 0)
            {
                // Добавление нового автора
                string insert = "INSERT INTO Authors (Name, DateOfBirth) VALUES (@Name, @DateOfBirth)";
                DbHelper.Execute(insert,
                    new SqlParameter("@Name", name),
                    new SqlParameter("@DateOfBirth", dobValue));
            }
            else
            {
                // Редактирование существующего автора
                string update = "UPDATE Authors SET Name=@Name, DateOfBirth=@DateOfBirth WHERE Id=@Id";
                DbHelper.Execute(update,
                    new SqlParameter("@Name", name),
                    new SqlParameter("@DateOfBirth", dobValue),
                    new SqlParameter("@Id", _authorId));
            }

            this.DialogResult = true;
        }

        // Обработка кнопки "Отмена"
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}