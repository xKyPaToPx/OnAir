using System.Windows;
using OnAir.Models;

namespace OnAir.Views
{
    public partial class UserEditDialog : Window
    {
        public User User { get; private set; }

        public UserEditDialog(User user = null)
        {
            InitializeComponent();
            RoleComboBox.ItemsSource = System.Enum.GetValues(typeof(UserRole));
            if (user != null)
            {
                UsernameTextBox.Text = user.Username;
                PasswordBox.Password = user.Password;
                FullNameTextBox.Text = user.FullName;
                RoleComboBox.SelectedItem = user.Role;
            }
            else
            {
                RoleComboBox.SelectedIndex = 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Логин и пароль обязательны для заполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            User = new User
            {
                Username = UsernameTextBox.Text,
                Password = PasswordBox.Password,
                FullName = FullNameTextBox.Text,
                Role = (UserRole)RoleComboBox.SelectedItem
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
} 