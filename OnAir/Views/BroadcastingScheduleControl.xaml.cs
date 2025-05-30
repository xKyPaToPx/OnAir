using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OnAir.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace OnAir.Views
{
    public partial class BroadcastingScheduleControl : UserControl
    {
        private readonly AppDbContext _context;
        private DateTime _selectedDate;
        private ObservableCollection<Broadcast> _schedule = new ObservableCollection<Broadcast>();
        private readonly Regex _timeRegex = new Regex(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$");
        private TimeSpan _startTime;

        // Предполагаемая продолжительность рекламного блока. Вам может потребоваться настроить это.
        private readonly TimeSpan AdvertisementDuration = TimeSpan.FromMinutes(1);

        public BroadcastingScheduleControl()
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
            string newText = textBox.Text;
            int caretIndex = textBox.CaretIndex;
            if (e.Text == ":")
            {
                if (newText.Contains(":"))
                {
                    e.Handled = true;
                    return;
                }
                if (newText.Count(char.IsDigit) < 2)
                {
                    e.Handled = true;
                    return;
                }
            }
            else if (!char.IsDigit(e.Text[0]))
            {
                e.Handled = true;
                return;
            }
            newText = newText.Insert(caretIndex, e.Text);
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
            var newBroadcast = new Broadcast
            {
                Date = DateOnly.FromDateTime(_selectedDate),
                Items = new List<BroadcastItem> { selectedItem }
            };
            _schedule.Add(newBroadcast);
            var availableItems = ((List<BroadcastItem>)AvailableItemsListBox.ItemsSource).ToList();
            availableItems.Remove(selectedItem);
            AvailableItemsListBox.ItemsSource = availableItems;
            UpdateScheduleTimes();
        }

        private void LoadData()
        {
            try
            {
                var date = DateOnly.FromDateTime(_selectedDate);
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
                    _startTime = broadcast.StartTime;
                    StartTimeTextBox.Text = _startTime.ToString(@"hh\:mm");
                }
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
            if (DateTime.TryParse(ScheduleDatePicker.Text, out DateTime date))
            {
                _selectedDate = date;
                LoadData();
            }
        }

        private void ScheduleDatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
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
            if (selectedIndex == -1)
            {
                MessageBox.Show("Пожалуйста, выберите элемент для перемещения",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            if (selectedIndex == -1)
            {
                MessageBox.Show("Пожалуйста, выберите элемент для перемещения",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (selectedIndex < _schedule.Count - 1)
            {
                var item = _schedule[selectedIndex];
                _schedule.RemoveAt(selectedIndex);
                _schedule.Insert(selectedIndex + 1, item);
                ScheduleListBox.SelectedIndex = selectedIndex + 1;
                UpdateScheduleTimes();
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
                    return;
                }
                else
                {
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
                var droppedBroadcast = e.Data.GetData(typeof(Broadcast)) as Broadcast;
                if (droppedBroadcast != null)
                {
                    var point = e.GetPosition(ScheduleListBox);
                    var element = ScheduleListBox.InputHitTest(point) as DependencyObject;
                    while (element != null && !(element is DataGridRow))
                        element = VisualTreeHelper.GetParent(element);
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
                var existingBroadcast = _context.Broadcasts
                    .Include(b => b.Items)
                    .FirstOrDefault(b => b.Date == date);
                if (existingBroadcast != null)
                {
                    existingBroadcast.StartTime = _startTime;
                    if (!string.IsNullOrEmpty(PlannedEndTimeTextBox.Text) && _timeRegex.IsMatch(PlannedEndTimeTextBox.Text))
                    {
                        existingBroadcast.PlannedEndTime = TimeSpan.Parse(PlannedEndTimeTextBox.Text);
                    }
                    existingBroadcast.Items.Clear();
                    foreach (var b in _schedule)
                    {
                        if (b.Items != null && b.Items.Count > 0)
                        {
                            existingBroadcast.Items.Add(b.Items[0]);
                        }
                    }
                }
                else
                {
                    var newBroadcast = new Broadcast
                    {
                        Date = date,
                        StartTime = _startTime,
                        PlannedEndTime = !string.IsNullOrEmpty(PlannedEndTimeTextBox.Text) && _timeRegex.IsMatch(PlannedEndTimeTextBox.Text)
                            ? TimeSpan.Parse(PlannedEndTimeTextBox.Text)
                            : (TimeSpan?)null,
                        Items = new List<BroadcastItem>()
                    };
                    foreach (var b in _schedule)
                    {
                        if (b.Items != null && b.Items.Count > 0)
                        {
                            newBroadcast.Items.Add(b.Items[0]);
                        }
                    }
                    _context.Broadcasts.Add(newBroadcast);
                }
                _context.SaveChanges();
                MessageBox.Show("Расписание успешно сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                var endTime = !string.IsNullOrEmpty(PlannedEndTimeTextBox.Text) && _timeRegex.IsMatch(PlannedEndTimeTextBox.Text)
                    ? TimeSpan.Parse(PlannedEndTimeTextBox.Text)
                    : TimeSpan.FromHours(23);

                var scheduler = new BroadcastScheduler(_context, _startTime, endTime);
                var newSchedule = scheduler.GenerateSchedule(DateOnly.FromDateTime(_selectedDate));

                _schedule.Clear();
                foreach (var broadcast in newSchedule)
                {
                    _schedule.Add(broadcast);
                }

                UpdateScheduleTimes();
                MessageBox.Show("Расписание успешно сгенерировано!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации расписания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 
