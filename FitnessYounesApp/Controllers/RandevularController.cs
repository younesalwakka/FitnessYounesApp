using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;
using FitnessYounesApp.Models;

namespace FitnessYounesApp.Controllers
{
    public class RandevularController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevularController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        // ✅ STEP 1: Get gyms that offer the selected service (by Hizmet name)
        [HttpGet]
        public async Task<IActionResult> GetSporSalonlariByHizmet(int hizmetId)
        {
            // 1) get selected hizmet name
            var hizmetAd = await _context.Hizmetler
                .Where(h => h.Id == hizmetId)
                .Select(h => h.Ad)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(hizmetAd))
                return Json(new object[] { });

            // 2) get all gyms that have a hizmet with same name
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
            // نجيب اسم الخدمة
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
                    adSoyad = ah.Antrenor!.Ad + " " + ah.Antrenor!.Soyad   // ✅ بدل AdSoyad
                })
                .Distinct()
                .OrderBy(x => x.adSoyad)
                .ToListAsync();

            return Json(antrenorler);
        }





        // GET: Randevular
        // هنا نستخدم Include حتى نجلب بيانات الانترنور والخدمة والعضو مع الحجز
        public async Task<IActionResult> Index()
        {
            var randevular = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil);

            return View(await randevular.ToListAsync());
        }

        // GET: Randevular/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // GET: Randevular/Create
        // GET: Randevular/Create
        public IActionResult Create()
        {
            // 1) الخدمة تظهر عادي
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad");

            // 2) النادي والمدربين: فارغين بالبداية (سنملأهم لاحقًا بالـ JavaScript)
            ViewData["SporSalonuId"] = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewData["AntrenorId"] = new SelectList(Enumerable.Empty<SelectListItem>());

            // 3) العضو عادي
            ViewData["UyeProfilId"] = new SelectList(_context.UyeProfiller, "Id", "AdSoyad");

            return View();
        }


        // POST: Randevular/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AntrenorId,HizmetId,UyeProfilId,Baslangic,Bitis,Ucret,Durum")] Randevu randevu)
        {
            if (ModelState.IsValid)
            {
                // 1) Double booking kontrolu:
                // Aynı antrenor icin, zaman araliklari cakisiyor mu?
                bool cakisanRandevuVar = await _context.Randevular
                    .AnyAsync(r =>
                        r.AntrenorId == randevu.AntrenorId &&     // ayni antrenor
                        r.Baslangic < randevu.Bitis &&            // eski baslangic < yeni bitis
                        r.Bitis > randevu.Baslangic               // eski bitis > yeni baslangic
                    );

                if (cakisanRandevuVar)
                {
                    // Genel bir model hatasi ekliyoruz, sayfanin ustundeki validation summary'de gozukur
                    ModelState.AddModelError(string.Empty,
                        "Secilen zaman araliginda bu antrenor icin baska bir randevu zaten var.");
                }
                else
                {
                    _context.Add(randevu);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Eger hata olursa dropdown'lari yeniden doldurmamiz lazim
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "Ad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            ViewData["UyeProfilId"] = new SelectList(_context.UyeProfiller, "Id", "AdSoyad", randevu.UyeProfilId);

            return View(randevu);
        }

        // GET: Randevular/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
            {
                return NotFound();
            }

            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            ViewData["UyeProfilId"] = new SelectList(_context.UyeProfiller, "Id", "AdSoyad", randevu.UyeProfilId);

            return View(randevu);
        }

        // POST: Randevular/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AntrenorId,HizmetId,UyeProfilId,Baslangic,Bitis,Ucret,Durum")] Randevu randevu)
        {
            if (id != randevu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 1) Double booking kontrolu (Edit icin):
                bool cakisanRandevuVar = await _context.Randevular
                    .AnyAsync(r =>
                        r.AntrenorId == randevu.AntrenorId &&
                        r.Id != randevu.Id &&                    // kendisini hariç tut
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
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "Ad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "Id", "Ad", randevu.HizmetId);
            ViewData["UyeProfilId"] = new SelectList(_context.UyeProfiller, "Id", "AdSoyad", randevu.UyeProfilId);

            return View(randevu);
        }

        // GET: Randevular/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (randevu == null)
            {
                return NotFound();
            }

            return View(randevu);
        }

        // POST: Randevular/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.Id == id);
        }
    }
}
