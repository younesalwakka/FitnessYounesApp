using System.Collections.Generic;

namespace FitnessYounesApp.ViewModels
{
    public class AntrenorHizmetSecimViewModel
    {
        public int AntrenorId { get; set; }
        public string AntrenorAdSoyad { get; set; }

        public List<HizmetSecimItem> Hizmetler { get; set; } = new();
    }

    public class HizmetSecimItem
    {
        public int HizmetId { get; set; }
        public string HizmetAd { get; set; }
        public bool SeciliMi { get; set; }
    }
}

