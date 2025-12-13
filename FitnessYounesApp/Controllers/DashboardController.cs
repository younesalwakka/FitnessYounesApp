using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;

namespace FitnessYounesApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = new DashboardViewModel
            {
                ToplamSporSalonu = await _context.SporSalonlari.CountAsync(),
                ToplamAntrenor = await _context.Antrenorler.CountAsync(),
                ToplamUye = await _context.UyeProfiller.CountAsync(),
                ToplamRandevu = await _context.Randevular.CountAsync(),
                BekleyenRandevular = await _context.Randevular
                    .Where(r => r.Durum == Models.RandevuDurumu.Beklemede)
                    .CountAsync(),
                OnaylananRandevular = await _context.Randevular
                    .Where(r => r.Durum == Models.RandevuDurumu.Onaylandi)
                    .CountAsync(),
                ToplamHizmet = await _context.Hizmetler.CountAsync(),
                BuAyRandevular = await _context.Randevular
                    .Where(r => r.Baslangic.Month == DateTime.Now.Month && 
                                r.Baslangic.Year == DateTime.Now.Year)
                    .CountAsync()
            };

            // Son randevular
            dashboardData.SonRandevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .OrderByDescending(r => r.Baslangic)
                .Take(5)
                .ToListAsync();

            // En çok randevu alan antrenörler
            dashboardData.PopulerAntrenorler = await _context.Antrenorler
                .Select(a => new PopulerAntrenorViewModel
                {
                    AntrenorAdi = a.AdSoyad,
                    RandevuSayisi = a.Randevular.Count
                })
                .OrderByDescending(a => a.RandevuSayisi)
                .Take(5)
                .ToListAsync();

            return View(dashboardData);
        }
    }

    public class DashboardViewModel
    {
        public int ToplamSporSalonu { get; set; }
        public int ToplamAntrenor { get; set; }
        public int ToplamUye { get; set; }
        public int ToplamRandevu { get; set; }
        public int BekleyenRandevular { get; set; }
        public int OnaylananRandevular { get; set; }
        public int ToplamHizmet { get; set; }
        public int BuAyRandevular { get; set; }
        public List<Models.Randevu> SonRandevular { get; set; } = new();
        public List<PopulerAntrenorViewModel> PopulerAntrenorler { get; set; } = new();
    }

    public class PopulerAntrenorViewModel
    {
        public string AntrenorAdi { get; set; } = string.Empty;
        public int RandevuSayisi { get; set; }
    }
}

