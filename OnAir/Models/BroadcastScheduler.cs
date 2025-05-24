using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using OnAir.Models;

namespace OnAir.Models
{
    public class BroadcastScheduler
    {
        private readonly AppDbContext _context;
        private readonly TimeSpan _defaultStartTime;
        private readonly TimeSpan _defaultEndTime;

        public BroadcastScheduler(AppDbContext context, TimeSpan defaultStartTime, TimeSpan defaultEndTime)
        {
            _context = context;
            _defaultStartTime = defaultStartTime;
            _defaultEndTime = defaultEndTime;
        }

        public List<Broadcast> GenerateSchedule(DateOnly date)
        {
            var schedule = new List<Broadcast>();
            var currentTime = _defaultStartTime;

            // 1. Получаем и фильтруем доступные элементы
            var availableItems = _context.BroadcastItems
                .Where(i => i.BroadcastId == null)
                .ToList();
                

            if (!availableItems.Any())
            {
                return schedule;
            }

            var (mainPrograms, availableAds) = FilterAvailableItems(availableItems);

            // Множество для отслеживания уже добавленных уникальных программ/серий.
            var addedUniquePrograms = new HashSet<string>();

            // 2. Строим упорядоченный список кандидатов на добавление
            var programCandidates = BuildProgramCandidates(mainPrograms, addedUniquePrograms);

            // 3. Итерируемся по кандидатам и добавляем в расписание с проверками и рекламой
            foreach (var item in programCandidates)
            {
                // Проверяем, помещается ли текущий элемент в расписание
                if (!TryAddItemToSchedule(schedule, item, ref currentTime, _defaultEndTime))
                {
                    // Если элемент не помещается, прекращаем добавление
                    break;
                }

                // Добавляем рекламу после элемента, если возможно
                TryAddAdvertisementAfterItem(schedule, availableAds, item.AgeLimit, ref currentTime, _defaultEndTime);
            }

            return schedule;
        }

        // Вспомогательный метод для фильтрации доступных элементов
        private (List<BroadcastItem> mainPrograms, List<BroadcastItem> availableAds) FilterAvailableItems(List<BroadcastItem> items)
        {
            var mainPrograms = items
                .Where(i => i.BroadcastItemType != BroadcastItemType.Advertising)
                .ToList()
                .DistinctBy(b => $"{b.Title}{b.Series}{b.Part}")
                .ToList();

            var availableAds = items
                .Where(i => i.BroadcastItemType == BroadcastItemType.Advertising)
                .ToList();

            return (mainPrograms, availableAds);
        }

        // Вспомогательный метод для построения упорядоченного списка кандидатов
        private List<BroadcastItem> BuildProgramCandidates(List<BroadcastItem> mainPrograms, HashSet<string> addedUniquePrograms)
        {
            var programCandidates = new List<BroadcastItem>();

            // Группируем программы по названию
            var programGroupsByTitle = mainPrograms
                .GroupBy(p => p.Title)
                .ToList();

            var temp = CollectionsMarshal.AsSpan(programGroupsByTitle);
            Random.Shared.Shuffle(temp);
            programGroupsByTitle = temp.ToArray().ToList();
            foreach (var group in programGroupsByTitle)
            {
                var programsInGroup = group.ToList();

                // Группируем по сериям внутри группы (для сериалов)
                var serialGroups = programsInGroup
                    .Where(p => p.Series.HasValue)
                    .GroupBy(p => p.Series.Value)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(p => p.Part ?? 0).ToList()
                    );

                // Одиночные программы в этой группе
                var standaloneItems = programsInGroup
                    .Where(p => !p.Series.HasValue)
                    .ToList();

                // Обрабатываем сериальные группы
                
                var serialGroup = serialGroups.FirstOrDefault();
                var seriesNumber = serialGroup.Key;
                var parts = serialGroup.Value;

                if (!parts.Any()) continue;

                // Определяем уникальный ключ для этой серии
                string uniqueKey = $"{group.Key}_S{seriesNumber}";

                // Если эта серия уже была добавлена в кандидаты, пропускаем
                if (addedUniquePrograms.Contains(uniqueKey))
                {
                    continue;
                }

                // Добавляем все части этой серии в список кандидатов в правильном порядке
                programCandidates.AddRange(parts);

                // Добавляем ключ серии в множество уникальных
                addedUniquePrograms.Add(uniqueKey);
                

                // Обрабатываем одиночные программы
                foreach (var item in standaloneItems)
                {
                     // Определяем уникальный ключ для этой одиночной программы
                    uniqueKey = item.Title;

                    // Если одиночная программа с таким названием уже была добавлена в кандидаты, пропускаем
                    if (addedUniquePrograms.Contains(uniqueKey))
                    {
                        continue;
                    }

                    // Добавляем одиночную программу в список кандидатов
                    programCandidates.Add(item);

                    // Добавляем ключ одиночной программы в множество уникальных
                    addedUniquePrograms.Add(uniqueKey);
                }
            }

             // Опционально: Отсортировать programCandidates если нужен специфический общий порядок (например, по названию)
             // В текущей реализации порядок определяется порядком групп по названию, затем серий внутри групп, затем одиночных.
             // Если нужен другой порядок (например, сначала все одиночные, потом все сериалы), логику сбора кандидатов нужно изменить.

            return programCandidates;
        }

        // Вспомогательный метод для попытки добавить элемент в расписание
        private bool TryAddItemToSchedule(List<Broadcast> schedule, BroadcastItem item, ref TimeSpan currentTime, TimeSpan endTime)
        {
            // Проверяем, помещается ли элемент в расписание
            if (currentTime.Add(item.Duration) > endTime)
            {
                return false; // Не помещается
            }

            // Добавляем элемент в расписание
            schedule.Add(new Broadcast
            {
                Date = DateOnly.FromDateTime(DateTime.Today), // Используем текущую дату из контекста GenerateSchedule
                Items = new List<BroadcastItem> { item }
            });

            // Обновляем текущее время
            currentTime = currentTime.Add(item.Duration);

            return true; // Успешно добавлено
        }

        // Вспомогательный метод для попытки найти и добавить рекламу
        private void TryAddAdvertisementAfterItem(List<Broadcast> schedule, List<BroadcastItem> availableAds, int previousItemAgeLimit, ref TimeSpan currentTime, TimeSpan endTime)
        {
             if (availableAds.Any())
            {
                // Ищем рекламный элемент с подходящим возрастным ограничением
                var suitableAd = availableAds.FirstOrDefault(ad => ad.AgeLimit <= previousItemAgeLimit);

                if (suitableAd != null)
                {
                    // Проверяем, помещается ли реклама в расписание
                    if (currentTime.Add(suitableAd.Duration) <= endTime)
                    {
                        schedule.Add(new Broadcast
                        {
                            Date = DateOnly.FromDateTime(DateTime.Today), // Используем текущую дату
                            Items = new List<BroadcastItem> { suitableAd }
                        });
                        currentTime = currentTime.Add(suitableAd.Duration);
                        availableAds.Remove(suitableAd); // Удаляем использованную рекламу
                    }
                }
            }
        }
    }
} 