namespace Zoomag.Views;

using System.Windows;
using Models;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        RoleComboBox.ItemsSource = Enum.GetValues(typeof(UserRole));
        RoleComboBox.SelectedIndex = 0;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var login = LoginTextBox.Text;

        if (RoleComboBox.SelectedItem is UserRole selectedRole)
        {
            var password = PasswordBox.Password;

            if (IsValidUser(login, password, selectedRole))
            {
                Window mainWindow = selectedRole == UserRole.Admin ? new AdminWindow() : new SellerWindow();

                mainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Введен не верный логин или пароль.", "Неверные данные для входа", MessageBoxButton.OK);
                PasswordBox.Clear();
            }
        }
        else
        {
            MessageBox.Show("Выберите роль", "Роль не выбрана", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private bool IsValidUser(string login, string password, UserRole role)
    {
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            return false;

        return (role == UserRole.Admin && password == "admin") || (role == UserRole.Seller && password == "123");
    }
}
