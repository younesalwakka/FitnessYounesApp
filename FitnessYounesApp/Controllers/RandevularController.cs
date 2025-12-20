using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;
using FitnessYounesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FitnessYounesApp.Controllers
{
    public class RandevularController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RandevularController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // AJAX HELPERS
        // =========================================================

        // ✅ STEP 1: Get gyms that offer the selected service (by Hizmet name)
        [HttpGet]
        public async Task<IActionResult> GetSporSalonlariByHizmet(int hizmetId)
        {
            var hizmetAd = await _context.Hizmetler
                .Where(h => h.Id == hizmetId)
                .Select(h => h.Ad)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(hizmetAd))
                return Json(Array.Empty<object>());

            var salonlar = await _context.Hizmetler
                .Where(h => h.Ad == hizmetAd)
                .Select(h => new
                {
                    id = h.SporSalonuId,
                    ad = h.SporSalonu!.Ad
                })
                .Distinct()
                .OrderBy(x => x.ad)
                .ToListAsync();

            return Json(salonlar);
        }

        [HttpGet]
        public async Task<IActionResult> GetAntrenorlerByHizmetVeSalon(int hizmetId, int sporSalonuId)
        {
            var hizmetAd = await _context.Hizmetler
                .Where(h => h.Id == hizmetId)
                .Select(h => h.Ad)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(hizmetAd))
                return Json(Array.Empty<object>());

            var antrenorler = await _context.AntrenorHizmetleri
                .Where(ah =>
                    ah.Hizmet!.Ad == hizmetAd &&
                    ah.Antrenor!.SporSalonuId == sporSalonuId
                )
                .Select(ah => new
                {
                    id = ah.AntrenorId,
                    adSoyad = ah.Antrenor!.Ad + " " + ah.Antrenor!.Soyad
                })
                .Distinct()
                .OrderBy(x => x.adSoyad)
                .ToListAsync();

            return Json(antrenorler);
        }

        // ✅ السعر يظهر في الفورم فقط (عرض) لكن لا نعتمد عليه بالحفظ
        [HttpGet]
        public async Task<IActionResult> GetUcretByHizmet(int hizmetId)
        {
            var ucret = await _context.Hizmetler
                .Where(h => h.Id == hizmetId)
                .Select(h => h.Ucret)
                .FirstOrDefaultAsync();

            return Json(new { ucret });
        }

        // =========================================================
        // ADMIN PAGES
        // =========================================================

        // ✅ Admin: عرض جميع المواعيد
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var randevular = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil);

            return View(await randevular.ToListAsync());
        }

        // ✅ Admin: التفاصيل
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // ✅ Admin: تعديل (اختياري)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "Ad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            ViewData["UyeProfilId"] = new SelectList(_context.UyeProfiller, "Id", "AdSoyad", randevu.UyeProfilId);

            return View(randevu);
        }

        // ✅ Admin: حفظ التعديل
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AntrenorId,HizmetId,UyeProfilId,Baslangic,Bitis,Ucret,Durum")] Randevu randevu)
        {
            if (id != randevu.Id) return NotFound();

            if (ModelState.IsValid)
            {
                bool cakisanRandevuVar = await _context.Randevular
                    .AnyAsync(r =>
                        r.AntrenorId == randevu.AntrenorId &&
                        r.Id != randevu.Id &&
                        r.Baslangic < randevu.Bitis &&
                        r.Bitis > randevu.Baslangic
                    );

                if (cakisanRandevuVar)
                {
                    ModelState.AddModelError(string.Empty,
                        "Secilen zaman araliginda bu antrenor icin baska bir randevu zaten var.");
                }
                else
                {
                    try
                    {
                        _context.Update(randevu);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!RandevuExists(randevu.Id))
                            return NotFound();
                        throw;
                    }
                }
            }

            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "Ad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            ViewData["UyeProfilId"] = new SelectList(_context.UyeProfiller, "Id", "AdSoyad", randevu.UyeProfilId);

            return View(randevu);
        }

        // ✅ Admin: حذف (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // ✅ Admin: حذف (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
                _context.Randevular.Remove(randevu);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // MEMBER PAGES
        // =========================================================

        // ✅ Member: Create page
        [Authorize]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            var hasProfile = await _context.UyeProfiller.AnyAsync(u => u.IdentityUserId == userId);
            if (!hasProfile)
            {
                TempData["Error"] = "Randevu oluşturmak için önce üye profili oluşturmalısınız.";
                return RedirectToAction("Create", "UyeProfiller");
            }

            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");
            ViewData["SporSalonuId"] = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewData["AntrenorId"] = new SelectList(Enumerable.Empty<SelectListItem>());

            return View();
        }

        // ✅ Member: Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("AntrenorId,HizmetId")] Randevu randevu, DateTime RandevuTarihSaat)
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            var uyeProfilId = await _context.UyeProfiller
                .Where(u => u.IdentityUserId == userId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (uyeProfilId == 0)
            {
                TempData["Error"] = "Randevu oluşturmak için önce üye profili oluşturmalısınız.";
                return RedirectToAction("Create", "UyeProfiller");
            }

            // ✅ Baslangic from form
            randevu.Baslangic = RandevuTarihSaat;

            // ✅ Get price + duration from DB
            var hizmetData = await _context.Hizmetler
                .Where(h => h.Id == randevu.HizmetId)
                .Select(h => new { h.Ucret, h.SureDakika })
                .FirstOrDefaultAsync();

            if (hizmetData == null)
            {
                ModelState.AddModelError("", "Hizmet bulunamadı.");
            }
            else if (hizmetData.SureDakika <= 0)
            {
                ModelState.AddModelError("", "Hizmet süresi bulunamadı.");
            }
            else
            {
                randevu.Ucret = hizmetData.Ucret;
                randevu.Bitis = randevu.Baslangic.AddMinutes(hizmetData.SureDakika);
            }

            // ✅ Set profile + status
            randevu.UyeProfilId = uyeProfilId;
            randevu.Durum = RandevuDurumu.Beklemede;

            // ✅ Availability + Conflict checks
            if (ModelState.IsValid)
            {
                // ✅ Appointment must be within same day (avoid crossing midnight)
                if (randevu.Bitis.Date != randevu.Baslangic.Date)
                {
                    ModelState.AddModelError("", "Randevu aynı gün içinde olmalıdır.");
                }
                else
                {
                    // ✅ 1) Check trainer availability (DayOfWeek + time range)
                    var gun = randevu.Baslangic.DayOfWeek;
                    var startTime = randevu.Baslangic.TimeOfDay;
                    var endTime = randevu.Bitis.TimeOfDay;

                    bool musaitMi = await _context.AntrenorMusaitlikler.AnyAsync(m =>
                        m.AntrenorId == randevu.AntrenorId
                        && m.Gun == gun
                        && m.BaslangicSaati <= startTime
                        && m.BitisSaati >= endTime
                    );

                    if (!musaitMi)
                    {
                        ModelState.AddModelError("", "Seçilen saat aralığında antrenör müsait değil.");
                    }

                    // ✅ 2) Check overlap with existing appointments (real overlap)
                    bool cakismaVar = await _context.Randevular.AnyAsync(r =>
                        r.AntrenorId == randevu.AntrenorId
                        && r.Baslangic < randevu.Bitis
                        && r.Bitis > randevu.Baslangic
                    );

                    if (cakismaVar)
                    {
                        ModelState.AddModelError("", "Bu saat aralığında antrenör için başka bir randevu zaten var.");
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Add(randevu);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(MyRandevular));
                }
            }

            // Reload dropdowns on error
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            ViewData["SporSalonuId"] = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewData["AntrenorId"] = new SelectList(Enumerable.Empty<SelectListItem>());

            return View(randevu);
        }

        // ✅ Member: MyRandevular
        [Authorize]
        public async Task<IActionResult> MyRandevular()
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            var uyeProfilId = await _context.UyeProfiller
                .Where(u => u.IdentityUserId == userId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (uyeProfilId == 0)
                return View(new List<Randevu>());

            var randevular = await _context.Randevular
                .Where(r => r.UyeProfilId == uyeProfilId)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.Baslangic)
                .ToListAsync();

            return View(randevular);
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.Id == id);
        }
    }
}
