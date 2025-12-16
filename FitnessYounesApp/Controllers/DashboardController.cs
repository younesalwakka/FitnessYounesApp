using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;
using FitnessYounesApp.Models;

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
            var model = new DashboardViewModel
            {
                ToplamSporSalonu = await _context.SporSalonlari.CountAsync(),
                ToplamAntrenor = await _context.Antrenorler.CountAsync(),
                ToplamUye = await _context.UyeProfiller.CountAsync(),
                ToplamRandevu = await _context.Randevular.CountAsync(),

                BekleyenRandevular = await _context.Randevular
                    .Where(r => r.Durum == RandevuDurumu.Beklemede)
                    .CountAsync(),

                OnaylananRandevular = await _context.Randevular
                    .Where(r => r.Durum == RandevuDurumu.Onaylandi)
                    .CountAsync(),

                ToplamHizmet = await _context.Hizmetler.CountAsync(),

                BuAyRandevular = await _context.Randevular
                    .Where(r => r.Baslangic.Month == DateTime.Now.Month &&
                                r.Baslangic.Year == DateTime.Now.Year)
                    .CountAsync()
            };

            model.SonRandevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .OrderByDescending(r => r.Baslangic)
                .Take(5)
                .ToListAsync();

            model.PopulerAntrenorler = await _context.Antrenorler
                .Select(a => new PopulerAntrenorViewModel
                {
                    AntrenorAdi = a.AdSoyad,
                    RandevuSayisi = a.Randevular.Count
                })
                .OrderByDescending(a => a.RandevuSayisi)
                .Take(5)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDurum(int randevuId, int durum)
        {
            var randevu = await _context.Randevular.FirstOrDefaultAsync(r => r.Id == randevuId);
            if (randevu == null) return NotFound();

            if (!Enum.IsDefined(typeof(RandevuDurumu), durum))
                return BadRequest("Invalid Durum");

            randevu.Durum = (RandevuDurumu)durum;
            await _context.SaveChangesAsync();

            TempData["DurumMessage"] = $"Randevu durumu güncellendi: {randevu.Durum}";
            return RedirectToAction(nameof(Index));
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

        public List<Randevu> SonRandevular { get; set; } = new();
        public List<PopulerAntrenorViewModel> PopulerAntrenorler { get; set; } = new();
    }

    public class PopulerAntrenorViewModel
    {
        public string AntrenorAdi { get; set; } = string.Empty;
        public int RandevuSayisi { get; set; }
    }
}