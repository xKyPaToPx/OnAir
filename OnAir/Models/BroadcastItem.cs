using System;

namespace OnAir.Models
{
    public class BroadcastItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public BroadcastItemType BroadcastItemType { get; set; }
        public int? Series {  get; set; }
        public int? Part {  get; set; }
        public string? Rights { get; set; }
        public string? Customer { get; set; }
        public TimeSpan Duration { get; set; }
        public int AgeLimit { get; set; }
        public int? BroadcastId { get; set; }
        public Broadcast? Broadcast { get; set; }
        public int IndexInBroadcast { get; set; }
    }

    // Вспомогательный класс для хранения программы и её целевого времени
    public class ScheduleItemInfo
    {
        public BroadcastItem Item { get; set; }
        public TimeSpan? TargetTime { get; set; }
        public bool IsNews { get; set; }
    }

    public enum BroadcastItemType 
    {
        Default,
        Advertising
    }
} 