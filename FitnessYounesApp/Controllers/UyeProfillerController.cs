using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;
using FitnessYounesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FitnessYounesApp.Controllers
{
    public class UyeProfillerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UyeProfillerController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // ADMIN PAGES
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.UyeProfiller.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var uyeProfil = await _context.UyeProfiller.FirstOrDefaultAsync(m => m.Id == id);
            if (uyeProfil == null) return NotFound();

            return View(uyeProfil);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var uyeProfil = await _context.UyeProfiller.FindAsync(id);
            if (uyeProfil == null) return NotFound();

            return View(uyeProfil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AdSoyad,DogumTarihi,BoyCm,KiloKg,Cinsiyet")] UyeProfil formModel)
        {
            if (id != formModel.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(formModel);

            var uyeProfil = await _context.UyeProfiller.FindAsync(id);
            if (uyeProfil == null) return NotFound();

            uyeProfil.AdSoyad = formModel.AdSoyad;
            uyeProfil.DogumTarihi = formModel.DogumTarihi;
            uyeProfil.BoyCm = formModel.BoyCm;
            uyeProfil.KiloKg = formModel.KiloKg;
            uyeProfil.Cinsiyet = formModel.Cinsiyet;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var uyeProfil = await _context.UyeProfiller.FirstOrDefaultAsync(m => m.Id == id);
            if (uyeProfil == null) return NotFound();

            return View(uyeProfil);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uyeProfil = await _context.UyeProfiller.FindAsync(id);
            if (uyeProfil != null)
            {
                _context.UyeProfiller.Remove(uyeProfil);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // MEMBER PAGES
        // =========================================================

        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            var profil = await _context.UyeProfiller
                .FirstOrDefaultAsync(x => x.IdentityUserId == userId);

            if (profil == null)
                return RedirectToAction(nameof(Create));

            return RedirectToAction(nameof(MyDetails));
        }

        [Authorize]
        public async Task<IActionResult> MyDetails()
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            var profil = await _context.UyeProfiller
                .FirstOrDefaultAsync(x => x.IdentityUserId == userId);

            if (profil == null)
                return RedirectToAction(nameof(Create));

            return View("MyDetails", profil);
        }

        // ✅ MEMBER: PROFİL DÜZENLE (GET)
        [Authorize]
        public async Task<IActionResult> MyEdit()
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            var profil = await _context.UyeProfiller
                .FirstOrDefaultAsync(x => x.IdentityUserId == userId);

            if (profil == null)
                return RedirectToAction(nameof(Create));

            return View("MyEdit", profil);
        }

        // ✅ MEMBER: PROFİL DÜZENLE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> MyEdit([Bind("AdSoyad,DogumTarihi,BoyCm,KiloKg,Cinsiyet")] UyeProfil formModel)
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            var profil = await _context.UyeProfiller
                .FirstOrDefaultAsync(x => x.IdentityUserId == userId);

            if (profil == null)
                return RedirectToAction(nameof(Create));

            // ✅ Bu alanlar formdan gelmiyor
            ModelState.Remove("KullaniciId");
            ModelState.Remove("IdentityUserId");

            if (!ModelState.IsValid)
                return View("MyEdit", formModel);

            profil.AdSoyad = formModel.AdSoyad;
            profil.DogumTarihi = formModel.DogumTarihi;
            profil.BoyCm = formModel.BoyCm;
            profil.KiloKg = formModel.KiloKg;
            profil.Cinsiyet = formModel.Cinsiyet;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyDetails));
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            bool hasProfile = await _context.UyeProfiller.AnyAsync(x => x.IdentityUserId == userId);
            if (hasProfile)
                return RedirectToAction(nameof(MyProfile));

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("AdSoyad,DogumTarihi,BoyCm,KiloKg,Cinsiyet")] UyeProfil uyeProfil)
        {
            if (User.IsInRole("Admin"))
                return Forbid();

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Kullanıcı kimliği alınamadı. Lütfen tekrar giriş yapın.";
                // ✅ Identity login sayfası
                return Redirect("/Identity/Account/Login");
            }

            ModelState.Remove("KullaniciId");
            ModelState.Remove("IdentityUserId");

            bool hasProfile = await _context.UyeProfiller.AnyAsync(x => x.IdentityUserId == userId);
            if (hasProfile)
            {
                ModelState.AddModelError("", "Bu kullanıcı için zaten bir üye profili var.");
                return View(uyeProfil);
            }

            if (!ModelState.IsValid)
                return View(uyeProfil);

            uyeProfil.IdentityUserId = userId;
            uyeProfil.KullaniciId = userId; // مؤقتًا للتوافق مع القديم

            _context.Add(uyeProfil);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyProfile));
        }

        private bool UyeProfilExists(int id)
        {
            return _context.UyeProfiller.Any(e => e.Id == id);
        }
    }
}
