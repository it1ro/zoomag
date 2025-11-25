using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        // Заполняем ComboBox анонимными объектами с понятными названиями
        RoleComboBox.ItemsSource = Enum.GetValues<UserRole>()
            .Select(role => new
            {
                Value = role,
                DisplayName = GetEnumDescription(role)
            })
            .ToList();

        RoleComboBox.DisplayMemberPath = "DisplayName";
        RoleComboBox.SelectedValuePath = "Value";
        RoleComboBox.SelectedIndex = 0;
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

        // Используем SelectedValue — он уже будет типа UserRole
        if (RoleComboBox.SelectedValue is not UserRole selectedRole)
        {
            MessageBox.Show("Выберите роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();
        var user = context.User
            .FirstOrDefault(u => u.Login == login && u.Password == password && u.Role == selectedRole);

        if (user != null)
        {
            Window mainWindow = selectedRole switch
            {
                UserRole.Admin => new AdminWindow(),
                UserRole.Seller => new SellerWindow(),
                _ => throw new InvalidOperationException("Неизвестная роль")
            };

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

    // Вспомогательный метод для получения Description из enum
    private static string GetEnumDescription<T>(T value) where T : struct, Enum
    {
        var field = typeof(T).GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}
