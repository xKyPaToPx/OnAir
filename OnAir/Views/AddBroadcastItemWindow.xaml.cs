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

            // Настройка интерфейса в зависимости от режима
            if (_isAdvertisingMode)
            {
                Title = "Добавить рекламный ролик";
                PartsCountLabel.Text = "Количество дубликатов:";
                PartsCountTooltip.Text = "Введите количество дубликатов рекламы. Все дубликаты будут иметь часть = 1.";
                BroadcastItemTypeText.Text = "Реклама";
                SeriesTextBox.IsEnabled = false;
                SeriesTextBox.Text = "1";
            }
            else
            {
                Title = "Добавить элемент вещания";
                PartsCountLabel.Text = "Кол-во частей:";
                PartsCountTooltip.Text = "Введите количество частей. Будут созданы элементы с номерами частей от 1 до указанного числа.";
                BroadcastItemTypeText.Text = "Стандартный";
            }

            if (_isEditMode)
            {
                Title = "Редактировать элемент вещания";
                TitleTextBox.Text = item.Title;
                DescriptionTextBox.Text = item.Description;
                SeriesTextBox.Text = item.Series?.ToString() ?? "";
                PartsCountTextBox.Text = "1";
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
                PartsCountTextBox.Text = "1";
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

                if (!int.TryParse(PartsCountTextBox.Text, out int partsCount) || partsCount < 1)
                {
                    MessageBox.Show("Пожалуйста, введите корректное количество", 
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(HoursTextBox.Text, out int hours) || hours < 0)
                {
                    MessageBox.Show("Пожалуйста, введите корректное количество часов", 
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(MinutesTextBox.Text, out int minutes) || minutes < 0 || minutes > 59)
                {
                    MessageBox.Show("Пожалуйста, введите корректное количество минут (0-59)", 
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var duration = new TimeSpan(hours, minutes, 0);
                var ageLimit = int.Parse(((ComboBoxItem)AgeLimitComboBox.SelectedItem).Content.ToString().TrimEnd('+'));

                // Создаем элементы
                for (int i = 0; i < partsCount; i++)
                {
                    var item = new BroadcastItem
                    {
                        Title = TitleTextBox.Text,
                        Description = DescriptionTextBox.Text,
                        Series = !string.IsNullOrWhiteSpace(SeriesTextBox.Text) ? int.Parse(SeriesTextBox.Text) : null,
                        Part = _isAdvertisingMode ? 1 : i + 1, // Для рекламы всегда часть = 1
                        Rights = RightsTextBox.Text,
                        Customer = CustomerTextBox.Text,
                        Duration = duration,
                        AgeLimit = ageLimit,
                        BroadcastItemType = _isAdvertisingMode ? BroadcastItemType.Advertising : BroadcastItemType.Default
                    };

                    if (_item != null && i == 0)
                    {
                        item.Id = _item.Id;
                        _context.BroadcastItems.Update(item);
                    }
                    else
                    {
                        _context.BroadcastItems.Add(item);
                    }
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