namespace FitnessYounesApp.Models
{
    public class AntrenorMusaitlik
    {
        public int Id { get; set; }

        public int AntrenorId { get; set; }
        public Antrenor? Antrenor { get; set; }

        public DayOfWeek Gun { get; set; }

        public TimeSpan BaslangicSaati { get; set; }

        public TimeSpan BitisSaati { get; set; }
    }
}

