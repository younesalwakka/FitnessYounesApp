using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;
using FitnessYounesApp.Models;
using FitnessYounesApp.ViewModels;

namespace FitnessYounesApp.Controllers
{
    public class AntrenorlerController : Controller
    {
        private readonly ApplicationDbContext _context;

        // 1) Constructor
        // يستقبل DbContext عن طريق DI
        public AntrenorlerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 2) Index
        // عرض قائمة جميع المدربين مع اسم الصالة
        public async Task<IActionResult> Index()
        {
            var antrenorler = _context.Antrenorler
                .Include(a => a.SporSalonu);

            return View(await antrenorler.ToListAsync());
        }

        // 3) Details
        // عرض تفاصيل مدرب واحد
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // 4) Create (GET)
        // عرض فورم اضافة مدرب جديد + قائمة بالصالات
        public IActionResult Create()
        {
            ViewData["SporSalonuId"] = new SelectList(
                _context.SporSalonlari,
                "Id",
                "Ad"
            );

            return View();
        }

        // 5) Create (POST)
        // حفظ المدرب الجديد في قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Antrenor antrenor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(antrenor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SporSalonuId"] = new SelectList(
                _context.SporSalonlari,
                "Id",
                "Ad",
                antrenor.SporSalonuId
            );

            return View(antrenor);
        }

        // 6) Edit (GET)
        // عرض بيانات المدرب في نموذج التعديل
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewData["SporSalonuId"] = new SelectList(
                _context.SporSalonlari,
                "Id",
                "Ad",
                antrenor.SporSalonuId
            );

            return View(antrenor);
        }

        // 7) Edit (POST)
        // حفظ التعديلات على المدرب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Antrenor antrenor)
        {
            if (id != antrenor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["SporSalonuId"] = new SelectList(
                _context.SporSalonlari,
                "Id",
                "Ad",
                antrenor.SporSalonuId
            );

            return View(antrenor);
        }

        // 8) Delete (GET)
        // عرض صفحة تأكيد حذف المدرب
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // 9) DeleteConfirmed (POST)
        // تنفيذ عملية الحذف فعليا
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 10) HizmetleriDuzenle (GET)
        // عرض كل الخدمات على شكل checkboxes لمدرب معيّن
        public async Task<IActionResult> HizmetleriDuzenle(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            var tumHizmetler = await _context.Hizmetler.ToListAsync();

            var seciliHizmetIds = antrenor.AntrenorHizmetleri
                .Select(ah => ah.HizmetId)
                .ToHashSet();

            var vm = new AntrenorHizmetSecimViewModel
            {
                AntrenorId = antrenor.Id,
                AntrenorAdSoyad = antrenor.Ad + " " + antrenor.Soyad,
                Hizmetler = tumHizmetler.Select(h => new HizmetSecimItem
                {
                    HizmetId = h.Id,
                    HizmetAd = h.Ad,
                    SeciliMi = seciliHizmetIds.Contains(h.Id)
                }).ToList()
            };

            return View(vm);
        }

        // 11) HizmetleriDuzenle (POST)
        // استلام الخدمات المختارة وتحديث جدول AntrenorHizmetleri
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetleriDuzenle(int antrenorId, int[] seciliHizmetler)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorHizmetleri)
                .FirstOrDefaultAsync(a => a.Id == antrenorId);

            if (antrenor == null) return NotFound();

            // حذف العلاقات القديمة
            _context.AntrenorHizmetleri.RemoveRange(antrenor.AntrenorHizmetleri);

            // اضافة العلاقات الجديدة
            if (seciliHizmetler != null)
            {
                foreach (var hizmetId in seciliHizmetler)
                {
                    _context.AntrenorHizmetleri.Add(new AntrenorHizmet
                    {
                        AntrenorId = antrenorId,
                        HizmetId = hizmetId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = antrenorId });
        }

        // دالة مساعدة: هل يوجد مدرب بهذا الـ Id؟
        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.Id == id);
        }
    }
}
