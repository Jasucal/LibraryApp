using LibraryApp.DataAccess;
using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LibraryApp
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
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

        // Обработка кнопки "Зарегистрироваться"
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            DateTime? dob = dpDateOfBirth.SelectedDate;
            string phone = txtPhoneNumber.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();
            int role = 2; // User

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

            // Проверка телефона
            if (!Regex.IsMatch(phone, @"^\+7-\d{3}-\d{3}-\d{2}-\d{2}$"))
            {
                MessageBox.Show("Телефон должен быть в формате +7-XXX-XXX-XX-XX");
                return;
            }

            // Проверка Email
            if (!email.Contains("@"))
            {
                MessageBox.Show("Email должен содержать символ @.");
                return;
            }

            // Добавление пользователя
            string insert = @"INSERT INTO Readers
                (FullName, DateOfBirth, PhoneNumber, Email, Password, Role)
                VALUES (@FullName, @DateOfBirth, @PhoneNumber, @Email, @Password, @Role)";

            DbHelper.Execute(insert,
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@DateOfBirth", dob.Value),
                new SqlParameter("@PhoneNumber", phone),
                new SqlParameter("@Email", email),
                new SqlParameter("@Password", password),
                new SqlParameter("@Role", role));

            MessageBox.Show("Регистрация успешна!");
            DialogResult = true;
        }

        // Обработка кнопки "Отмена"
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}