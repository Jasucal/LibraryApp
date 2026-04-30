using System;
using System.Windows;

namespace LibraryApp
{
    public partial class ReaderInfoWindow : Window
    {
        // Информация о конкретном пользователе
        public ReaderInfoWindow(string fullName, DateTime? dateOfBirth, string phone, string email)
        {
            InitializeComponent();

            txtFullName.Text = fullName;
            txtDateOfBirth.Text = dateOfBirth.HasValue ? dateOfBirth.Value.ToString("dd.MM.yyyy") : "";
            txtPhone.Text = phone;
            txtEmail.Text = email;
        }

        // Обработка кнопки "Отменить"
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}