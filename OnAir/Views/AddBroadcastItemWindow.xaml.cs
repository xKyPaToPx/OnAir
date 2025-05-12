using System;
using System.Windows;
using System.Windows.Controls;
using OnAir.Models;

namespace OnAir.Views
{
    public partial class AddBroadcastItemWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly BroadcastItem _item;
        private readonly bool _isEditMode;
        private readonly bool _isAdvertisingMode;

        public AddBroadcastItemWindow(BroadcastItem item = null, bool isAdvertisingMode = false)
        {
            InitializeComponent();
            _context = new AppDbContext();
            _item = item;
            _isEditMode = item != null;
            _isAdvertisingMode = isAdvertisingMode;

            if (_isEditMode)
            {
                Title = "Редактировать элемент вещания";
                TitleTextBox.Text = item.Title;
                DescriptionTextBox.Text = item.Description;
                SeriesTextBox.Text = item.Series?.ToString() ?? "";
                PartTextBox.Text = item.Part?.ToString() ?? "";
                RightsTextBox.Text = item.Rights ?? "";
                CustomerTextBox.Text = item.Customer ?? "";
                BroadcastItemTypeText.Text = item.BroadcastItemType.ToString();
                
                // Установка значений длительности
                HoursTextBox.Text = item.Duration.Hours.ToString();
                MinutesTextBox.Text = item.Duration.Minutes.ToString();
                
                // Установка выбранного возрастного ограничения
                string ageLimitText = $"{item.AgeLimit}+";
                foreach (ComboBoxItem comboItem in AgeLimitComboBox.Items)
                {
                    if (comboItem.Content.ToString() == ageLimitText)
                    {
                        AgeLimitComboBox.SelectedItem = comboItem;
                        break;
                    }
                }
            }
            else
            {
                AgeLimitComboBox.SelectedIndex = 0;
                HoursTextBox.Text = "0";
                MinutesTextBox.Text = "0";
                BroadcastItemTypeText.Text = _isAdvertisingMode ? "Реклама" : "Стандартный";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, введите название", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(CountTextBox.Text, out int count) || count < 1)
                {
                    MessageBox.Show("Пожалуйста, введите корректное количество элементов (минимум 1)", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int hours = 0;
                int minutes = 0;

                if (!string.IsNullOrWhiteSpace(HoursTextBox.Text))
                {
                    if (!int.TryParse(HoursTextBox.Text, out hours) || hours < 0)
                    {
                        MessageBox.Show("Пожалуйста, введите корректное количество часов", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(MinutesTextBox.Text))
                {
                    if (!int.TryParse(MinutesTextBox.Text, out minutes) || minutes < 0 || minutes > 59)
                    {
                        MessageBox.Show("Пожалуйста, введите корректное количество минут (0-59)", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var duration = new TimeSpan(hours, minutes, 0);

                for (int i = 0; i < count; i++)
                {
                    var item = new BroadcastItem
                    {
                        Title = TitleTextBox.Text,
                        Description = DescriptionTextBox.Text,
                        Series = !string.IsNullOrWhiteSpace(SeriesTextBox.Text) ? int.Parse(SeriesTextBox.Text) : null,
                        Part = !string.IsNullOrWhiteSpace(PartTextBox.Text) ? int.Parse(PartTextBox.Text) : null,
                        Rights = RightsTextBox.Text,
                        Customer = CustomerTextBox.Text,
                        Duration = duration,
                        AgeLimit = int.Parse(((ComboBoxItem)AgeLimitComboBox.SelectedItem).Content.ToString().TrimEnd('+')),
                        BroadcastItemType = _isAdvertisingMode ? BroadcastItemType.Advertising : BroadcastItemType.Default
                    };

                    _context.BroadcastItems.Add(item);
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 