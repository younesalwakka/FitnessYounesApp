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
    public class AntrenorMusaitliklerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorMusaitliklerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AntrenorMusaitlikler
        public async Task<IActionResult> Index()
        {
            var musaitlikler = _context.AntrenorMusaitlikler
                .Include(m => m.Antrenor);   // نجلب بيانات المدرب أيضًا

            return View(await musaitlikler.ToListAsync());
        }


        // GET: AntrenorMusaitlikler/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var musaitlik = await _context.AntrenorMusaitlikler
                .Include(m => m.Antrenor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (musaitlik == null)
            {
                return NotFound();
            }

            return View(musaitlik);
        }


        // GET: AntrenorMusaitlikler/Create
        public IActionResult Create()
        {
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad");
            return View();
        }


        // POST: AntrenorMusaitlikler/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AntrenorId,Gun,BaslangicSaati,BitisSaati")] AntrenorMusaitlik antrenorMusaitlik)
        {
            if (ModelState.IsValid)
            {
                _context.Add(antrenorMusaitlik);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "Id", antrenorMusaitlik.AntrenorId);
            return View(antrenorMusaitlik);
        }

        // GET: AntrenorMusaitlikler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenorMusaitlik = await _context.AntrenorMusaitlikler.FindAsync(id);
            if (antrenorMusaitlik == null)
            {
                return NotFound();
            }
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "Id", antrenorMusaitlik.AntrenorId);
            return View(antrenorMusaitlik);
        }

        // POST: AntrenorMusaitlikler/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AntrenorId,Gun,BaslangicSaati,BitisSaati")] AntrenorMusaitlik antrenorMusaitlik)
        {
            if (id != antrenorMusaitlik.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenorMusaitlik);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorMusaitlikExists(antrenorMusaitlik.Id))
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
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "Id", "AdSoyad", antrenorMusaitlik.AntrenorId);
            return View(antrenorMusaitlik);
        }

        // GET: AntrenorMusaitlikler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var antrenorMusaitlik = await _context.AntrenorMusaitlikler
                .Include(a => a.Antrenor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (antrenorMusaitlik == null)
            {
                return NotFound();
            }

            return View(antrenorMusaitlik);
        }

        // POST: AntrenorMusaitlikler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenorMusaitlik = await _context.AntrenorMusaitlikler.FindAsync(id);
            if (antrenorMusaitlik != null)
            {
                _context.AntrenorMusaitlikler.Remove(antrenorMusaitlik);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorMusaitlikExists(int id)
        {
            return _context.AntrenorMusaitlikler.Any(e => e.Id == id);
        }
    }
}
