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
    public class SporSalonlariController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ------------------------------------------------------
        // Constructor
        // المهمة: تهيئة الـ DbContext حتى نتمكن من التعامل
        // مع قاعدة البيانات (قراءة، إضافة، تعديل، حذف).
        // هذا يسمى Dependency Injection
        // ------------------------------------------------------


        public SporSalonlariController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------
        // Index Action
        // المهمة: عرض قائمة جميع الصالات الرياضية (Spor Salonları)
        // يقوم بجلب البيانات من قاعدة البيانات باستخدام ToListAsync()
        // ثم يقوم بإرسالها إلى View (صفحة Index)
        // هذا يمثل جزء "Read" من CRUD
        // ------------------------------------------------------

        // GET: SporSalonus
        public async Task<IActionResult> Index()
        {
            return View(await _context.SporSalonlari.ToListAsync());
        }

        // GET: SporSalonus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sporSalonu = await _context.SporSalonlari
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sporSalonu == null)
            {
                return NotFound();
            }

            return View(sporSalonu);
        }

        // GET: SporSalonus/Create
        // ------------------------------------------------------
        // Create (GET)
        // المهمة: عرض صفحة إنشاء صالة جديدة
        // هذا الأكشن لا يتعامل مع قاعدة البيانات
        // فقط يعرض نموذج الإدخال للمستخدم
        // يمثل الجزء الأول من عملية "Create" في CRUD
        // ------------------------------------------------------
        public IActionResult Create()
        {
            return View();
        }

        // POST: SporSalonus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        // ------------------------------------------------------
        // Create (POST)
        // المهمة: استقبال البيانات المُدخلة من المستخدم وحفظها في قاعدة البيانات
        // ModelState.IsValid يتحقق من صحة البيانات قبل الحفظ
        // _context.Add(sporSalonu) لإضافة السجل إلى الذاكرة
        // SaveChangesAsync لحفظ البيانات فعليًا في SQL Server
        // بعد الحفظ يتم إعادة التوجيه إلى Index
        // ------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Adres,Telefon,CalismaSaatleri")] SporSalonu sporSalonu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sporSalonu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sporSalonu);
        }
        // ------------------------------------------------------
        // Edit (GET)
        // المهمة: جلب بيانات الصالة المطلوبة بواسطة ID
        // الهدف: عرض البيانات الحالية داخل نموذج التعديل
        // إذا لم يتم العثور على الصالة → NotFound()
        // هذا يمثل الجزء الأول من "Update" في CRUD
        // ------------------------------------------------------

        // GET: SporSalonus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sporSalonu = await _context.SporSalonlari.FindAsync(id);
            if (sporSalonu == null)
            {
                return NotFound();
            }
            return View(sporSalonu);
        }

        // ------------------------------------------------------
        // Edit (POST)
        // المهمة: استقبال البيانات المعدّلة من المستخدم
        // _context.Update(sporSalonu) لتحديث السجل في الذاكرة
        // SaveChangesAsync لحفظ التعديلات فعليًا في قاعدة البيانات
        // جزء أساسي من عملية "Update" في CRUD
        // ------------------------------------------------------

        // POST: SporSalonus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Adres,Telefon,CalismaSaatleri")] SporSalonu sporSalonu)
        {
            if (id != sporSalonu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sporSalonu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SporSalonuExists(sporSalonu.Id))
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
            return View(sporSalonu);
        }

        // GET: SporSalonus/Delete/5
        // ------------------------------------------------------
        // Delete (GET)
        // المهمة: عرض صفحة تأكيد الحذف
        // هنا لا يتم الحذف فعليًا — فقط يتم عرض البيانات للمستخدم
        // يمثل المرحلة الأولى من "Delete" في CRUD
        // ------------------------------------------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sporSalonu = await _context.SporSalonlari
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sporSalonu == null)
            {
                return NotFound();
            }

            return View(sporSalonu);
        }


        // ------------------------------------------------------
        // DeleteConfirmed (POST)
        // المهمة: تنفيذ عملية الحذف الفعلية من قاعدة البيانات
        // Remove() لحذف السجل من الذاكرة
        // SaveChangesAsync لحذف السجل بشكل نهائي من SQL Server
        // بعد الحذف → العودة إلى صفحة Index
        // ------------------------------------------------------

        // POST: SporSalonus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sporSalonu = await _context.SporSalonlari.FindAsync(id);
            if (sporSalonu != null)
            {
                _context.SporSalonlari.Remove(sporSalonu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SporSalonuExists(int id)
        {
            return _context.SporSalonlari.Any(e => e.Id == id);
        }
    }
}
