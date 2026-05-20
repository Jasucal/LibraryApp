using System;
using System.Data;
using System.Windows;
using System.Data.SqlClient;
using LibraryApp.DataAccess;

namespace LibraryApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Обработка кнопки войти
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = tbEmail.Text.Trim();
            string pass = tbPassword.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Введите Email и пароль.");
                return;
            }

            string query = "SELECT * FROM Readers WHERE Email=@Email AND Password=@Password";

            DataTable dt = DbHelper.GetData(query,
                new SqlParameter("@Email", email),
                new SqlParameter("@Password", pass));

            if (dt.Rows.Count == 1)
            {
                DataRow row = dt.Rows[0];
                int role = Convert.ToInt32(row["Role"]);   // 1 - админ, 0 - пользователь
                int userId = Convert.ToInt32(row["Id"]);   // ID текущего пользователя

                if (role == 1)
                {
                    MainWindow mw = new MainWindow(userId);
                    mw.Show();
                }
                else
                {
                    UserMainWindow umw = new UserMainWindow(userId);
                    umw.Show();
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный Email или пароль.");
            }
        }

        // Обработка ссылки на переход к регистрации
        private void HyperlinkRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            bool? result = registerWindow.ShowDialog();

            if (result == true)
            {
                MessageBox.Show("Регистрация успешно завершена! Теперь войдите с новым аккаунтом.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}