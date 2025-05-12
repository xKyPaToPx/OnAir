using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OnAir.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace OnAir.Views
{
    public partial class BroadcastingScheduleWindow : Window
    {
        private readonly AppDbContext _context;
        private DateTime _selectedDate;
        private ObservableCollection<Broadcast> _schedule = new ObservableCollection<Broadcast>();
        private readonly Regex _timeRegex = new Regex(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$");
        private TimeSpan _startTime;

        public BroadcastingScheduleWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _selectedDate = DateTime.Today;
            ScheduleDatePicker.SelectedDate = _selectedDate;
            _schedule = new ObservableCollection<Broadcast>();
            ScheduleListBox.ItemsSource = _schedule;
            _startTime = TimeSpan.Parse("07:00");
            LoadData();
        }

        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            if (!_timeRegex.IsMatch(newText))
            {
                e.Handled = true;
            }
        }

        private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (!_timeRegex.IsMatch(textBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите время в формате ЧЧ:ММ (например, 07:00)", 
                    "Неверный формат", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Text = "07:00";
                _startTime = TimeSpan.Parse("07:00");
            }
            else
            {
                _startTime = TimeSpan.Parse(textBox.Text);
                UpdateScheduleTimes();
            }
        }

        private void StartTimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Получаем текущий текст и добавляем новый символ
            string newText = textBox.Text;
            int caretIndex = textBox.CaretIndex;
            
            // Если это двоеточие
            if (e.Text == ":")
            {
                // Если уже есть двоеточие, запрещаем ввод
                if (newText.Contains(":"))
                {
                    e.Handled = true;
                    return;
                }
                // Если меньше двух цифр, запрещаем ввод
                if (newText.Count(char.IsDigit) < 2)
                {
                    e.Handled = true;
                    return;
                }
            }
            // Если это не цифра и не двоеточие
            else if (!char.IsDigit(e.Text[0]))
            {
                e.Handled = true;
                return;
            }

            // Проверяем, что после ввода получится корректное время
            newText = newText.Insert(caretIndex, e.Text);
            
            // Если текст пустой или содержит только цифры, разрешаем ввод
            if (string.IsNullOrEmpty(newText) || newText.All(c => char.IsDigit(c) || c == ':'))
            {
                return;
            }

            if (!_timeRegex.IsMatch(newText))
            {
                e.Handled = true;
            }
        }

        private void StartTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (!_timeRegex.IsMatch(textBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите время в формате ЧЧ:ММ (например, 07:00)", 
                    "Неверный формат", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Text = _startTime.ToString(@"hh\:mm");
            }
            else
            {
                _startTime = TimeSpan.Parse(textBox.Text);
                UpdateScheduleTimes();
            }
        }

        private void PlannedEndTimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Если текст пустой или содержит только цифры и двоеточие, разрешаем ввод
            if (string.IsNullOrEmpty(newText) || newText.All(c => char.IsDigit(c) || c == ':'))
            {
                return;
            }

            if (!_timeRegex.IsMatch(newText))
            {
                e.Handled = true;
            }
        }

        private void PlannedEndTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (!_timeRegex.IsMatch(textBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите время в формате ЧЧ:ММ (например, 23:00)",
                    "Неверный формат", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Text = "";
            }
            // Здесь можно добавить сохранение значения в модель Broadcast, если нужно
        }

        private void UpdateScheduleTimes()
        {
            TimeSpan currentTime = _startTime;
            foreach (var broadcast in _schedule)
            {
                if (broadcast.Items != null && broadcast.Items.Count > 0)
                {
                    broadcast.StartTime = currentTime;
                    currentTime = currentTime.Add(broadcast.Items[0].Duration);
                }
            }
            ScheduleListBox.Items.Refresh();
        }

        private void AddToScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = AvailableItemsListBox.SelectedItem as BroadcastItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите элемент для добавления в расписание", 
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Удаляем из доступных и добавляем в расписание (только в памяти)
            var newBroadcast = new Broadcast
            {
                Date = DateOnly.FromDateTime(_selectedDate),
                Items = new List<BroadcastItem> { selectedItem }
            };
            _schedule.Add(newBroadcast);
            var availableItems = ((List<BroadcastItem>)AvailableItemsListBox.ItemsSource).ToList();
            availableItems.Remove(selectedItem);
            AvailableItemsListBox.ItemsSource = availableItems;

            // Пересчитываем время для всех элементов
            UpdateScheduleTimes();
        }

        private void LoadData()
        {
            try
            {
                var date = DateOnly.FromDateTime(_selectedDate);
                // Находим Broadcast на выбранную дату
                var broadcast = _context.Broadcasts
                    .Include(b => b.Items)
                    .FirstOrDefault(b => b.Date == date);

                _schedule.Clear();
                List<BroadcastItem> scheduledItems = new List<BroadcastItem>();
                if (broadcast != null)
                {
                    scheduledItems = broadcast.Items.OrderBy(i => i.IndexInBroadcast).ToList();
                    foreach (var item in scheduledItems)
                    {
                        _schedule.Add(new Broadcast
                        {
                            Date = date,
                            Items = new List<BroadcastItem> { item }
                        });
                    }
                    // Загружаем сохраненное время начала
                    _startTime = broadcast.StartTime;
                    StartTimeTextBox.Text = _startTime.ToString(@"hh\:mm");
                }

                // Остальные элементы — в доступные
                var scheduledIds = scheduledItems.Select(i => i.Id).ToList();
                var availableItems = _context.BroadcastItems
                    .Where(i => i.BroadcastId.Equals(null))
                    .ToList();
                AvailableItemsListBox.ItemsSource = availableItems;

                UpdateScheduleTimes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveFromScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBroadcast = ScheduleListBox.SelectedItem as Broadcast;
            if (selectedBroadcast == null)
            {
                MessageBox.Show("Пожалуйста, выберите элемент для удаления из расписания", 
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Удаляем из расписания и возвращаем элемент в доступные (только в памяти)
            _schedule.Remove(selectedBroadcast);
            var availableItems = ((List<BroadcastItem>)AvailableItemsListBox.ItemsSource).ToList();
            if (selectedBroadcast.Items != null && selectedBroadcast.Items.Count > 0)
            {
                availableItems.Add(selectedBroadcast.Items[0]);
            }
            AvailableItemsListBox.ItemsSource = availableItems;

            UpdateScheduleTimes();
        }

        private void ScheduleDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScheduleDatePicker.SelectedDate.HasValue)
            {
                _selectedDate = ScheduleDatePicker.SelectedDate.Value;
                LoadData();
            }
        }

        private void ScheduleDatePicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Позволяем пользователю вводить дату, не проверяя формат при каждом изменении
            if (DateTime.TryParse(ScheduleDatePicker.Text, out DateTime date))
            {
                _selectedDate = date;
                LoadData();
            }
        }

        private void ScheduleDatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            // Проверяем формат даты только когда пользователь закончил ввод
            if (!DateTime.TryParse(ScheduleDatePicker.Text, out DateTime date))
            {
                MessageBox.Show("Пожалуйста, введите дату в формате дд.мм.гггг", 
                    "Неверный формат", MessageBoxButton.OK, MessageBoxImage.Warning);
                ScheduleDatePicker.Text = _selectedDate.ToString("dd.MM.yyyy");
            }
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (_schedule == null) return;

            var selectedIndex = ScheduleListBox.SelectedIndex;
            if (selectedIndex > 0)
            {
                var item = _schedule[selectedIndex];
                _schedule.RemoveAt(selectedIndex);
                _schedule.Insert(selectedIndex - 1, item);
                ScheduleListBox.SelectedIndex = selectedIndex - 1;
                UpdateScheduleTimes();
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (_schedule == null) return;

            var selectedIndex = ScheduleListBox.SelectedIndex;
            if (selectedIndex < _schedule.Count - 1)
            {
                var item = _schedule[selectedIndex];
                _schedule.RemoveAt(selectedIndex);
                _schedule.Insert(selectedIndex + 1, item);
                ScheduleListBox.SelectedIndex = selectedIndex + 1;
                UpdateScheduleTimes();
            }
        }

        private void SaveScheduleOrder()
        {
            try
            {
                // Здесь можно добавить логику сохранения порядка элементов в базе данных
                // если это необходимо
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении порядка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AvailableItemsListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var selectedItem = AvailableItemsListBox.SelectedItem as BroadcastItem;
                if (selectedItem != null)
                {
                    System.Windows.DragDrop.DoDragDrop(AvailableItemsListBox, selectedItem, DragDropEffects.Move);
                }
            }
        }

        private void AvailableItemsListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(BroadcastItem)))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void AvailableItemsListBox_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var item = e.Data.GetData(typeof(BroadcastItem)) as BroadcastItem;
                if (item != null)
                {
                    // Если перетаскивается BroadcastItem обратно в доступные — ничего не делаем (он уже там)
                    return;
                }
                else
                {
                    // Если перетаскивается Broadcast из расписания в доступные
                    var broadcast = e.Data.GetData(typeof(Broadcast)) as Broadcast;
                    if (broadcast != null)
                    {
                        _schedule.Remove(broadcast);
                        var availableItems = ((List<BroadcastItem>)AvailableItemsListBox.ItemsSource).ToList();
                        if (broadcast.Items != null && broadcast.Items.Count > 0)
                        {
                            availableItems.Add(broadcast.Items[0]);
                        }
                        AvailableItemsListBox.ItemsSource = availableItems;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении элемента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScheduleListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var selectedBroadcast = ScheduleListBox.SelectedItem as Broadcast;
                if (selectedBroadcast != null)
                {
                    System.Windows.DragDrop.DoDragDrop(ScheduleListBox, selectedBroadcast, DragDropEffects.Move);
                }
            }
        }

        private void ScheduleListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Broadcast)))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void ScheduleListBox_Drop(object sender, DragEventArgs e)
        {
            if (_schedule == null) return;

            try
            {
                // Если перетаскивается BroadcastItem из доступных в расписание
                var droppedItem = e.Data.GetData(typeof(BroadcastItem)) as BroadcastItem;
                if (droppedItem != null)
                {
                    var newBroadcast = new Broadcast
                    {
                        Date = DateOnly.FromDateTime(_selectedDate),
                        Items = new List<BroadcastItem> { droppedItem }
                    };
                    _schedule.Add(newBroadcast);
                    var availableItems = ((List<BroadcastItem>)AvailableItemsListBox.ItemsSource).ToList();
                    availableItems.Remove(droppedItem);
                    AvailableItemsListBox.ItemsSource = availableItems;
                    UpdateScheduleTimes();
                    return;
                }

                // Если перетаскивается Broadcast внутри расписания (смена порядка)
                var droppedBroadcast = e.Data.GetData(typeof(Broadcast)) as Broadcast;
                if (droppedBroadcast != null)
                {
                    // Получаем позицию мыши
                    var point = e.GetPosition(ScheduleListBox);
                    var element = ScheduleListBox.InputHitTest(point) as DependencyObject;

                    // Ищем DataGridRow под курсором
                    while (element != null && !(element is DataGridRow))
                        element = VisualTreeHelper.GetParent(element);

                    // Если не на элемент, не делаем ничего
                    if (!(element is DataGridRow dataGridRow))
                        return;

                    var targetBroadcast = dataGridRow.DataContext as Broadcast;

                    if (droppedBroadcast != null && targetBroadcast != null)
                    {
                        int removedIdx = _schedule.IndexOf(droppedBroadcast);
                        int targetIdx = _schedule.IndexOf(targetBroadcast);

                        if (removedIdx < targetIdx)
                        {
                            _schedule.Insert(targetIdx + 1, droppedBroadcast);
                            _schedule.RemoveAt(removedIdx);
                        }
                        else
                        {
                            int remIdx = removedIdx + 1;
                            if (_schedule.Count + 1 > remIdx)
                            {
                                _schedule.Insert(targetIdx, droppedBroadcast);
                                _schedule.RemoveAt(remIdx);
                            }
                        }
                        UpdateScheduleTimes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при перемещении элемента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var date = DateOnly.FromDateTime(_selectedDate);

                // Находим существующее расписание на этот день
                var existingBroadcast = _context.Broadcasts
                    .Include(b => b.Items)
                    .FirstOrDefault(b => b.Date == date);

                if (existingBroadcast != null)
                {
                    // Обновляем время начала
                    existingBroadcast.StartTime = _startTime;

                    // Обновляем планируемое время окончания
                    if (!string.IsNullOrEmpty(PlannedEndTimeTextBox.Text) && _timeRegex.IsMatch(PlannedEndTimeTextBox.Text))
                    {
                        existingBroadcast.PlannedEndTime = TimeSpan.Parse(PlannedEndTimeTextBox.Text);
                    }
                    else
                    {
                        existingBroadcast.PlannedEndTime = null;
                    }

                    // Открепляем все существующие элементы
                    var existingItems = _context.BroadcastItems.Where(i => i.BroadcastId == existingBroadcast.Id).ToList();
                    foreach (var item in existingItems)
                    {
                        item.BroadcastId = null;
                        item.IndexInBroadcast = 0;
                    }
                }
                else
                {
                    // Создаём новый Broadcast
                    existingBroadcast = new Broadcast
                    {
                        Date = date,
                        StartTime = _startTime,
                        PlannedEndTime = (!string.IsNullOrEmpty(PlannedEndTimeTextBox.Text) && _timeRegex.IsMatch(PlannedEndTimeTextBox.Text))
                            ? TimeSpan.Parse(PlannedEndTimeTextBox.Text)
                            : null
                    };
                    _context.Broadcasts.Add(existingBroadcast);
                }
                _context.SaveChanges();

                // Привязываем все элементы расписания к Broadcast и выставляем IndexInBroadcast
                var allItems = _schedule.SelectMany(b => b.Items ?? new List<BroadcastItem>()).ToList();
                for (int i = 0; i < allItems.Count; i++)
                {
                    var item = allItems[i];
                    var dbItem = _context.BroadcastItems.FirstOrDefault(it => it.Id == item.Id);
                    if (dbItem != null)
                    {
                        dbItem.BroadcastId = existingBroadcast.Id;
                        dbItem.IndexInBroadcast = i;
                    }
                }
                _context.SaveChanges();

                MessageBox.Show("Расписание успешно сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении расписания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AutoFillButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем планируемое время окончания
                TimeSpan? plannedEndTime = null;
                if (!string.IsNullOrEmpty(PlannedEndTimeTextBox.Text) && _timeRegex.IsMatch(PlannedEndTimeTextBox.Text))
                {
                    plannedEndTime = TimeSpan.Parse(PlannedEndTimeTextBox.Text);
                }

                // Получаем список доступных элементов
                var availableItems = ((List<BroadcastItem>)AvailableItemsListBox.ItemsSource).ToList();
                if (!availableItems.Any())
                {
                    MessageBox.Show("Нет доступных элементов для добавления в расписание", 
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем список рекламных блоков
                var adItems = availableItems.Where(i => i.BroadcastItemType == BroadcastItemType.Advertising).ToList();
                if (!adItems.Any())
                {
                    MessageBox.Show("Нет доступных рекламных блоков", 
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Очищаем текущее расписание
                _schedule.Clear();

                // Группируем элементы по названию и серии
                var groupedItems = availableItems
                    .Where(i => i.BroadcastItemType != BroadcastItemType.Advertising)
                    .GroupBy(i => new { i.Title, i.Series })
                    .ToList();

                TimeSpan currentTime = _startTime;
                int adIndex = 0;

                foreach (var group in groupedItems)
                {
                    // Получаем все части для текущей группы
                    var parts = group.OrderBy(i => i.Part).ToList();
                    
                    // Добавляем все части подряд
                    foreach (var part in parts)
                    {
                        // Проверяем, не превышено ли планируемое время окончания
                        if (plannedEndTime.HasValue && currentTime >= plannedEndTime.Value)
                        {
                            return;
                        }

                        // Добавляем часть в расписание
                        _schedule.Add(new Broadcast
                        {
                            Date = DateOnly.FromDateTime(_selectedDate),
                            StartTime = currentTime,
                            Items = new List<BroadcastItem> { part }
                        });

                        currentTime = currentTime.Add(part.Duration);

                        // Добавляем рекламный блок после каждой части
                        if (adItems.Any())
                        {
                            var adItem = adItems[adIndex % adItems.Count];
                            _schedule.Add(new Broadcast
                            {
                                Date = DateOnly.FromDateTime(_selectedDate),
                                StartTime = currentTime,
                                Items = new List<BroadcastItem> { adItem }
                            });

                            currentTime = currentTime.Add(adItem.Duration);
                            adIndex++;
                        }
                    }
                }

                // Обновляем отображение
                ScheduleListBox.Items.Refresh();

                // Удаляем добавленные элементы из списка доступных
                var scheduledItems = _schedule.SelectMany(b => b.Items).ToList();
                availableItems.RemoveAll(i => scheduledItems.Contains(i));
                AvailableItemsListBox.ItemsSource = availableItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при автоматическом заполнении расписания: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 
