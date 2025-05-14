using System;
using System.Linq;
using System.Windows;
using OnAir.Models;
using Microsoft.EntityFrameworkCore;

namespace OnAir.Views
{
    public partial class BroadcastItemsWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly bool _isAdvertisingMode;
        private readonly bool _isAdminMode;

        public BroadcastItemsWindow(bool isAdvertisingMode = false, bool isAdminMode = false)
        {
            InitializeComponent();
            _context = new AppDbContext();
            _isAdvertisingMode = isAdvertisingMode;
            _isAdminMode = isAdminMode;

            if (_isAdvertisingMode)
            {
                Title = "Управление рекламными элементами";
            }
            else if (_isAdminMode)
            {
                Title = "Управление элементами вещания (Администратор)";
            }

            LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                var items = _context.BroadcastItems
                    .Include(i => i.Broadcast)
                    .OrderByDescending(i => i.BroadcastItemType)
                    .ToList();

                if (_isAdvertisingMode)
                {
                    ItemsDataGrid.ItemsSource = items.Where(i => i.BroadcastItemType == BroadcastItemType.Advertising);
                }
                else if (!_isAdminMode)
                {
                    ItemsDataGrid.ItemsSource = items.Where(i => i.BroadcastItemType != BroadcastItemType.Advertising);
                }
                else
                {
                    ItemsDataGrid.ItemsSource = items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddBroadcastItemWindow(isAdvertisingMode: _isAdvertisingMode, isAdmin: _isAdminMode);
            if (window.ShowDialog() == true)
            {
                if (_isAdvertisingMode)
                {
                    // Устанавливаем тип реклама для новых элементов в режиме рекламного отдела
                    var lastItem = _context.BroadcastItems.OrderByDescending(i => i.Id).FirstOrDefault();
                    if (lastItem != null)
                    {
                        lastItem.BroadcastItemType = BroadcastItemType.Advertising;
                        _context.SaveChanges();
                    }
                }
                LoadItems();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ItemsDataGrid.SelectedItem as BroadcastItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите элемент для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new AddBroadcastItemWindow(selectedItem, _isAdvertisingMode, _isAdminMode);
            if (editWindow.ShowDialog() == true)
            {
                LoadItems();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ItemsDataGrid.SelectedItem as BroadcastItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите элемент для удаления", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить элемент '{selectedItem.Title}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.BroadcastItems.Remove(selectedItem);
                    _context.SaveChanges();
                    LoadItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
} 