namespace ETL.Worker.Models
{
    public class TimeRecord
    {
        public int TimeKey { get; set; }
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int Day { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int WeekOfYear { get; set; }
        public bool IsWeekend { get; set; }
    }
}
