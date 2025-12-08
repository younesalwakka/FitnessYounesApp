using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;
using FitnessYounesApp.Models;

namespace FitnessYounesApp.Controllers
{
    public class UyeProfillerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UyeProfillerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UyeProfiller
        public async Task<IActionResult> Index()
        {
            return View(await _context.UyeProfiller.ToListAsync());
        }

        // GET: UyeProfiller/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uyeProfil = await _context.UyeProfiller
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uyeProfil == null)
            {
                return NotFound();
            }

            return View(uyeProfil);
        }

        // GET: UyeProfiller/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UyeProfiller/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KullaniciId,AdSoyad,DogumTarihi,BoyCm,KiloKg,Cinsiyet")] UyeProfil uyeProfil)
        {
            if (ModelState.IsValid)
            {
                _context.Add(uyeProfil);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(uyeProfil);
        }

        // GET: UyeProfiller/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uyeProfil = await _context.UyeProfiller.FindAsync(id);
            if (uyeProfil == null)
            {
                return NotFound();
            }
            return View(uyeProfil);
        }

        // POST: UyeProfiller/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KullaniciId,AdSoyad,DogumTarihi,BoyCm,KiloKg,Cinsiyet")] UyeProfil uyeProfil)
        {
            if (id != uyeProfil.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uyeProfil);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UyeProfilExists(uyeProfil.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(uyeProfil);
        }

        // GET: UyeProfiller/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uyeProfil = await _context.UyeProfiller
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uyeProfil == null)
            {
                return NotFound();
            }

            return View(uyeProfil);
        }

        // POST: UyeProfiller/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uyeProfil = await _context.UyeProfiller.FindAsync(id);
            if (uyeProfil != null)
            {
                _context.UyeProfiller.Remove(uyeProfil);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UyeProfilExists(int id)
        {
            return _context.UyeProfiller.Any(e => e.Id == id);
        }
    }
}
