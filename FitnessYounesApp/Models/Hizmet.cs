namespace FitnessYounesApp.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        public string Ad { get; set; } = null!;

        public string? Aciklama { get; set; }

        public int SureDakika { get; set; }

        public decimal Ucret { get; set; }

        public int SporSalonuId { get; set; }
        public SporSalonu? SporSalonu { get; set; }

        public ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
