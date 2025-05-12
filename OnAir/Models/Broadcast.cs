using System;
using System.Collections.Generic;

namespace OnAir.Models
{
    public class Broadcast
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public List<BroadcastItem> Items { get; set; } = new List<BroadcastItem>();
        public TimeSpan StartTime { get; set; }
        public TimeSpan? PlannedEndTime { get; set; }
    }
}
