using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OnAir.Models;
using Microsoft.EntityFrameworkCore;

namespace OnAir.Views
{
    public enum BroadcastItemsMode
    {
        Admin,
        Advertising,
        Broadcasting
    }

    public partial class BroadcastItemsControl : UserControl
    {
        private readonly AppDbContext _context;
        private readonly BroadcastItemsMode _mode;

        public BroadcastItemsControl(BroadcastItemsMode mode = BroadcastItemsMode.Admin)
        {
            InitializeComponent();
            _context = new AppDbContext();
            _mode = mode;
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

                if (_mode == BroadcastItemsMode.Advertising)
                {
                    ItemsDataGrid.ItemsSource = items.Where(i => i.BroadcastItemType == BroadcastItemType.Advertising).ToList();
                }
                else if (_mode == BroadcastItemsMode.Broadcasting)
                {
                    ItemsDataGrid.ItemsSource = items.Where(i => i.BroadcastItemType != BroadcastItemType.Advertising).ToList();
                }
                else // Admin
                {
                    ItemsDataGrid.ItemsSource = items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке элементов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddBroadcastItemWindow(
                isAdvertisingMode: _mode == BroadcastItemsMode.Advertising,
                isAdmin: _mode == BroadcastItemsMode.Admin
            );
            if (window.ShowDialog() == true)
            {
                LoadItems();
            }
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ItemsDataGrid.SelectedItem as BroadcastItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите элемент для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editWindow = new AddBroadcastItemWindow(selectedItem,
                isAdvertisingMode: _mode == BroadcastItemsMode.Advertising,
                isAdmin: _mode == BroadcastItemsMode.Admin
            );
            if (editWindow.ShowDialog() == true)
            {
                LoadItems();
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
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
                    MessageBox.Show($"Ошибка при удалении элемента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
} 