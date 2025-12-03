namespace FitnessYounesApp.Models
{
    public class SporSalonu
    {
        public int Id { get; set; }

        public string Ad { get; set; } = null!;

        public string? Adres { get; set; }

        public string? Telefon { get; set; }

        public string? CalismaSaatleri { get; set; }

        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();

        public ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();
    }
}
