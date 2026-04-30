using LibraryApp.DataAccess;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LibraryApp
{
    public partial class AddEditReaderWindow : Window
    {
        private int _readerId = 0;
        private int _currentRole = 2; // По умолчанию "User"

        public AddEditReaderWindow(int readerId = 0)
        {
            InitializeComponent();
            _readerId = readerId;

            if (_readerId > 0)
                LoadReaderData();
            else
            {
                _currentRole = 2; // Новый читатель всегда User
                UpdateRoleDisplay();
            }
        }

        private void LoadReaderData()
        {
            string query = "SELECT * FROM Readers WHERE Id=@Id";
            DataTable dt = DbHelper.GetData(query, new SqlParameter("@Id", _readerId));

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                txtFullName.Text = row["FullName"]?.ToString();

                if (row["DateOfBirth"] != DBNull.Value)
                    dpDateOfBirth.SelectedDate = Convert.ToDateTime(row["DateOfBirth"]);

                txtPhoneNumber.Text = row["PhoneNumber"]?.ToString();
                txtEmail.Text = row["Email"]?.ToString();
                txtPassword.Password = row["Password"]?.ToString();

                // Сохраняем и показываем роль
                _currentRole = row["Role"] != DBNull.Value ? Convert.ToInt32(row["Role"]) : 2;
                UpdateRoleDisplay();
            }
        }

        // Обновляем отображение роли
        private void UpdateRoleDisplay()
        {
            if (_currentRole == 1)
            {
                txtRole.Text = "🔑 Admin";
                txtRole.Foreground = Brushes.DarkRed;
            }
            else
            {
                txtRole.Text = "👤 User";
                txtRole.Foreground = Brushes.DarkGreen;
            }
        }

        // Блокировка цифр и символов в ФИО
        private void TxtFullName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[А-Яа-яA-Za-z\s]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        // Блокировка цифр и символов в ФИО вставкой
        private void TxtFullName_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                Regex regex = new Regex(@"^[А-Яа-яA-Za-z\s]+$");

                if (!regex.IsMatch(text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Обработка кнопки "Сохранить"
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            DateTime? dob = dpDateOfBirth.SelectedDate;
            string phone = txtPhoneNumber.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Проверка пустых полей
            if (string.IsNullOrWhiteSpace(fullName) ||
                !dob.HasValue ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Все поля должны быть заполнены.");
                return;
            }

            // Проверка даты рождения
            if (dob.HasValue)
            {
                DateTime today = DateTime.Today;
                int minYear = today.Year - 10;

                if (dob.Value > today)
                {
                    MessageBox.Show("Дата рождения не может быть больше текущей даты.");
                    return;
                }

                if (dob.Value.Year > minYear)
                {
                    MessageBox.Show("Читателю должно быть не меньше 10 лет.");
                    return;
                }

                if (dob.Value.Year < 1900)
                {
                    MessageBox.Show("Год рождения не может быть ниже 1900.");
                    return;
                }
            }

            // Телефон
            if (!Regex.IsMatch(phone, @"^\+7-\d{3}-\d{3}-\d{2}-\d{2}$"))
            {
                MessageBox.Show("Телефон должен быть в формате +7-XXX-XXX-XX-XX");
                return;
            }

            // Email
            if (!email.Contains("@"))
            {
                MessageBox.Show("Email должен содержать символ @.");
                return;
            }

            if (_readerId == 0) // Добавить нового читателя
            {
                string insert = @"INSERT INTO Readers
                    (FullName, DateOfBirth, PhoneNumber, Email, Password, Role)
                    VALUES (@FullName, @DateOfBirth, @PhoneNumber, @Email, @Password, @Role)";

                DbHelper.Execute(insert,
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@DateOfBirth", dob.Value),
                    new SqlParameter("@PhoneNumber", phone),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@Role", 2)); // Новый читатель всегда User
            }
            else // Редактировать существующего читателя
            {
                string update = @"UPDATE Readers
                    SET FullName=@FullName,
                        DateOfBirth=@DateOfBirth,
                        PhoneNumber=@PhoneNumber,
                        Email=@Email,
                        Password=@Password,
                        Role=@Role
                    WHERE Id=@Id";

                DbHelper.Execute(update,
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@DateOfBirth", dob.Value),
                    new SqlParameter("@PhoneNumber", phone),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@Role", _currentRole), // Сохраняем текущую роль
                    new SqlParameter("@Id", _readerId));
            }

            DialogResult = true;
        }

        // Обработка кнопки "Отменить"
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}