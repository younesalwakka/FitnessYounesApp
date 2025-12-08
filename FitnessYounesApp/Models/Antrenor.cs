namespace FitnessYounesApp.Models
{
    public class Antrenor
    {
        public int Id { get; set; }

        public string Ad { get; set; } = null!;

        public string Soyad { get; set; } = null!;

        public string? Biyografi { get; set; }

        public string? Uzmanlik { get; set; }

        public int SporSalonuId { get; set; }
        public SporSalonu? SporSalonu { get; set; }

        public ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();

        public ICollection<AntrenorMusaitlik> Musaitlikler { get; set; } = new List<AntrenorMusaitlik>();

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        public string TamAd => $"{Ad} {Soyad}";

        public string AdSoyad => $"{Ad} {Soyad}";

    }
}
