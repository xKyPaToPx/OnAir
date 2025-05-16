using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OnAir.Models;

namespace OnAir.Views
{
    public partial class UsersControl : UserControl
    {
        public UsersControl()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var db = new AppDbContext())
            {
                var users = db.Users.ToList();
                UsersDataGrid.ItemsSource = users;
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserEditDialog();
            if (dialog.ShowDialog() == true)
            {
                using (var db = new AppDbContext())
                {
                    db.Users.Add(dialog.User);
                    db.SaveChanges();
                }
                LoadUsers();
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                var dialog = new UserEditDialog(selectedUser);
                if (dialog.ShowDialog() == true)
                {
                    using (var db = new AppDbContext())
                    {
                        var user = db.Users.FirstOrDefault(u => u.Id == selectedUser.Id);
                        if (user != null)
                        {
                            user.Username = dialog.User.Username;
                            user.Password = dialog.User.Password;
                            user.Role = dialog.User.Role;
                            user.FullName = dialog.User.FullName;
                            db.SaveChanges();
                        }
                    }
                    LoadUsers();
                }
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (MessageBox.Show($"Удалить пользователя {selectedUser.Username}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var db = new AppDbContext())
                    {
                        var user = db.Users.FirstOrDefault(u => u.Id == selectedUser.Id);
                        if (user != null)
                        {
                            db.Users.Remove(user);
                            db.SaveChanges();
                        }
                    }
                    LoadUsers();
                }
            }
        }
    }
} 