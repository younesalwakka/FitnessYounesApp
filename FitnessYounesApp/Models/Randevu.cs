namespace FitnessYounesApp.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        public int AntrenorId { get; set; }
        public Antrenor? Antrenor { get; set; }

        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }

        public int UyeProfilId { get; set; }
        public UyeProfil? UyeProfil { get; set; }

        public DateTime Baslangic { get; set; }

        public DateTime Bitis { get; set; }

        public decimal Ucret { get; set; }

        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Beklemede;
    }
}
