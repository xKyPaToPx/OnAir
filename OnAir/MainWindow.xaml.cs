using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OnAir.Models;
using OnAir.Views;

namespace OnAir
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private UsersControl _usersControl;
        private BroadcastItemsControl _broadcastItemsControl;
        private BroadcastingScheduleControl _scheduleControl;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            SetUserInfo();

            // Выбор стартового контента по роли
            switch (_currentUser.Role)
            {
                case UserRole.Admin:
                    ShowUsersControl();
                    break;
                case UserRole.AdvertisingWorker:
                    ShowBroadcastItemsControl();
                    break;
                case UserRole.BroadcastingWorker:
                    ScheduleNavButton.Visibility = Visibility.Visible;
                    ScheduleNavButton_Click(null, null);
                    break;
                default:
                    // Можно показать заглушку или ничего
                    break;
            }
        }

        private void SetUserInfo()
        {
            // Аватарка — первая буква username (заглавная)
            if (!string.IsNullOrEmpty(_currentUser.Username))
                AvatarTextBlock.Text = _currentUser.Username.Substring(0, 1).ToUpper();
            else
                AvatarTextBlock.Text = "?";
            // Имя пользователя
            UserNameTextBlock.Text = _currentUser.FullName ?? _currentUser.Username;

            // Показываем нужные кнопки в зависимости от роли
            UsersNavButton.Visibility = Visibility.Collapsed;
            ItemsNavButton.Visibility = Visibility.Collapsed;
            ScheduleNavButton.Visibility = Visibility.Collapsed;

            switch (_currentUser.Role)
            {
                case UserRole.Admin:
                    UsersNavButton.Visibility = Visibility.Visible;
                    ItemsNavButton.Visibility = Visibility.Visible;
                    break;
                case UserRole.AdvertisingWorker:
                    ItemsNavButton.Visibility = Visibility.Visible;
                    break;
                case UserRole.BroadcastingWorker:
                    ItemsNavButton.Visibility = Visibility.Visible;
                    ScheduleNavButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void UsersNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUsersControl();
        }

        private void ItemsNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowBroadcastItemsControl();
        }

        private void ShowUsersControl()
        {
            if (_usersControl == null)
                _usersControl = new UsersControl();
            MainContentControl.Content = _usersControl;
        }

        private void ShowBroadcastItemsControl()
        {
            if (_broadcastItemsControl == null)
            {
                // Передаем режим в зависимости от роли
                if (_currentUser.Role == UserRole.Admin)
                    _broadcastItemsControl = new BroadcastItemsControl(BroadcastItemsMode.Admin);
                else if (_currentUser.Role == UserRole.AdvertisingWorker)
                    _broadcastItemsControl = new BroadcastItemsControl(BroadcastItemsMode.Advertising);
                else
                    _broadcastItemsControl = new BroadcastItemsControl(BroadcastItemsMode.Broadcasting);
            }
            MainContentControl.Content = _broadcastItemsControl;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void ScheduleNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scheduleControl == null)
                _scheduleControl = new BroadcastingScheduleControl();
            MainContentControl.Content = _scheduleControl;
        }
    }
}