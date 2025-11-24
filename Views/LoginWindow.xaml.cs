namespace Zoomag.Views;

using System.Windows;
using Zoomag.Data;
using Zoomag.Models;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        RoleComboBox.ItemsSource = Enum.GetValues(typeof(UserRole));
        RoleComboBox.SelectedIndex = 0; // Admin по умолчанию
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var login = LoginTextBox.Text.Trim();
        var password = PasswordBox.Password.Trim();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (RoleComboBox.SelectedItem is not UserRole selectedRole)
        {
            MessageBox.Show("Выберите роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();
        var user = context.User
            .FirstOrDefault(u => u.Login == login && u.Password == password && u.Role == selectedRole);

        if (user != null)
        {
            // Успешный вход
            Window mainWindow = selectedRole == UserRole.Admin
                ? new AdminWindow()
                : new SellerWindow();

            mainWindow.Show();
            Close();
        }
        else
        {
            MessageBox.Show("Неверный логин, пароль или роль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            PasswordBox.Clear();
            LoginTextBox.Focus();
        }
    }
}
