using Microsoft.AspNetCore.Identity;

namespace FitnessYounesApp.Models
{
    public class UyeProfil
    {
        public int Id { get; set; }

        // ✅ هذا الذي كانت Views تستخدمه سابقًا
        public string KullaniciId { get; set; } = null!;

        public string AdSoyad { get; set; } = null!;
        public DateTime? DogumTarihi { get; set; }
        public double? BoyCm { get; set; }
        public double? KiloKg { get; set; }
        public string? Cinsiyet { get; set; }

        // ✅ الربط الصحيح مع Identity
        public string? IdentityUserId { get; set; }   
        public IdentityUser? IdentityUser { get; set; } 

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
