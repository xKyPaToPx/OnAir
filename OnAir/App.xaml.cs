using System.Configuration;
using System.Data;
using System.Windows;
using OnAir.Views;

namespace OnAir
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Инициализация конфигурации и проверка строки подключения
                var connectionString = AppConfig.GetConnectionString();
                
                // Если строка подключения успешно получена, запускаем приложение
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при инициализации приложения: {ex.Message}\n\nПроверьте файл appsettings.json и настройки подключения к базе данных.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
