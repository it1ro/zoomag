using System;
using System.Windows;
using Zoomag.Models;

namespace Zoomag.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // ✅ Заполняем ComboBox значениями enum
            RoleComboBox.ItemsSource = Enum.GetValues(typeof(UserRole));
            RoleComboBox.SelectedIndex = 0; // Admin по умолчанию
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text;

            // ✅ Получаем выбранную роль как enum
            if (RoleComboBox.SelectedItem is UserRole selectedRole)
            {
                var password = PasswordBox.Password;

                if (IsValidUser(login, password, selectedRole))
                {
                    // ✅ Используем enum для определения окна
                    Window mainWindow = selectedRole == UserRole.Admin ?
                        new AdminWindow() :
                        new SellerWindow();

                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid credentials.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordBox.Clear();
                }
            }
            else
            {
                MessageBox.Show("Please select a role.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool IsValidUser(string login, string password, UserRole role)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                return false;

            // ✅ Проверяем enum
            return (role == UserRole.Admin || role == UserRole.Seller);
        }
    }
}
