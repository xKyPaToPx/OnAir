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

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Set user info
            UserInfoTextBlock.Text = $"{_currentUser.FullName} ({_currentUser.Role})";

            // Load role-specific content
            switch (_currentUser.Role)
            {
                case UserRole.Admin:
                    LoadAdminContent();
                    break;
                case UserRole.AdvertisingWorker:
                    LoadAdvertisingContent();
                    break;
                case UserRole.BroadcastingWorker:
                    LoadBroadcastingContent();
                    break;
            }
        }

        private void LoadAdminContent()
        {
            var adminPanel = new StackPanel();
            
            // Add admin-specific controls here
            var title = new System.Windows.Controls.TextBlock
            {
                Text = "Панель администратора",
                FontSize = 24,
                Margin = new Thickness(0, 0, 0, 20)
            };
            adminPanel.Children.Add(title);

            var manageUsersButton = new Button
            {
                Content = "Управление пользователями",
                Margin = new Thickness(0, 10, 0, 0),
                Style = (Style)Application.Current.FindResource("ModernButtonStyle")
            };
            manageUsersButton.Click += (s, e) => new UserManagementWindow().Show();
            adminPanel.Children.Add(manageUsersButton);

            var manageItemsButton = new Button
            {
                Content = "Управление элементами вещания",
                Margin = new Thickness(0, 10, 0, 0),
                Style = (Style)Application.Current.FindResource("ModernButtonStyle")
            };
            manageItemsButton.Click += (s, e) =>
            {
                var broadcastItemsWindow = new BroadcastItemsWindow(isAdminMode: true);
                broadcastItemsWindow.Owner = this;
                broadcastItemsWindow.ShowDialog();
            };
            adminPanel.Children.Add(manageItemsButton);

            // Add more admin controls as needed

            RoleContentControl.Content = adminPanel;
        }

        private void LoadAdvertisingContent()
        {
            var panel = new StackPanel { Margin = new Thickness(20) };
            
            var title = new TextBlock
            {
                Text = "Панель работника рекламного отдела",
                FontSize = 24,
                Margin = new Thickness(0, 0, 0, 20),
            };
            panel.Children.Add(title);

            var broadcastItemsButton = new Button
            {
                Content = "Управление элементами вещания",
                Margin = new Thickness(0, 0, 0, 10),
                Style = (Style)Application.Current.FindResource("ModernButtonStyle")
            };
            broadcastItemsButton.Click += (s, e) =>
            {
                var window = new BroadcastItemsWindow(true); // true означает, что окно открыто из рекламного отдела
                window.Owner = this;
                window.ShowDialog();
            };
            panel.Children.Add(broadcastItemsButton);

            RoleContentControl.Content = panel;
        }

        private void LoadBroadcastingContent()
        {
            var broadcastingPanel = new StackPanel();
            
            // Add broadcasting-specific controls here
            var title = new System.Windows.Controls.TextBlock
            {
                Text = "Панель работника вещания",
                FontSize = 24,
                Margin = new Thickness(0, 0, 0, 20)
            };
            broadcastingPanel.Children.Add(title);

            var manageScheduleButton = new Button
            {
                Content = "Управление расписанием",
                Margin = new Thickness(0, 10, 0, 0),
                Style = (Style)Application.Current.FindResource("ModernButtonStyle")
            };
            manageScheduleButton.Click += (s, e) => new BroadcastingScheduleWindow().Show();
            broadcastingPanel.Children.Add(manageScheduleButton);

            var manageItemsButton = new Button
            {
                Content = "Управление элементами вещания",
                Margin = new Thickness(0, 10, 0, 0),
                Style = (Style)Application.Current.FindResource("ModernButtonStyle")
            };
            manageItemsButton.Click += (s, e) =>
            {
                var broadcastItemsWindow = new BroadcastItemsWindow();
                broadcastItemsWindow.Owner = this;
                broadcastItemsWindow.ShowDialog();
            };
            broadcastingPanel.Children.Add(manageItemsButton);

            RoleContentControl.Content = broadcastingPanel;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}