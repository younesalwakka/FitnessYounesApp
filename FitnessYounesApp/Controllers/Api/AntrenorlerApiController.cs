using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;
using FitnessYounesApp.Models;

namespace FitnessYounesApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AntrenorlerApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AntrenorlerApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AntrenorlerApi
        // Tüm antrenörleri listeleme
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAntrenorler()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .Select(a => new
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    a.AdSoyad,
                    a.Uzmanlik,
                    a.Biyografi,
                    SporSalonuAdi = a.SporSalonu != null ? a.SporSalonu.Ad : null,
                    Hizmetler = a.AntrenorHizmetleri
                        .Where(ah => ah.Hizmet != null)
                        .Select(ah => new
                        {
                            ah.Hizmet!.Id,
                            ah.Hizmet.Ad
                        }).ToList()
                })
                .ToListAsync();

            return Ok(antrenorler);
        }

        // GET: api/AntrenorlerApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAntrenor(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    a.AdSoyad,
                    a.Uzmanlik,
                    a.Biyografi,
                    SporSalonuAdi = a.SporSalonu != null ? a.SporSalonu.Ad : null,
                    Hizmetler = a.AntrenorHizmetleri
                        .Where(ah => ah.Hizmet != null)
                        .Select(ah => new
                        {
                            ah.Hizmet!.Id,
                            ah.Hizmet.Ad
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (antrenor == null)
            {
                return NotFound();
            }

            return Ok(antrenor);
        }

        // GET: api/AntrenorlerApi/UygunAntrenorler?tarih=2025-12-15
        // Belirli bir tarihte uygun antrenörleri getirme
        [HttpGet("UygunAntrenorler")]
        public async Task<ActionResult<IEnumerable<object>>> GetUygunAntrenorler([FromQuery] DateTime tarih)
        {
            // Tarihin hangi günü olduğunu bul
            var gun = tarih.DayOfWeek;

            // O gün için müsaitlik tanımlı olan antrenörleri bul
            var uygunAntrenorler = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Include(a => a.Musaitlikler)
                .Include(a => a.Randevular)
                .Where(a => a.Musaitlikler.Any(m => m.Gun == gun))
                .Select(a => new
                {
                    a.Id,
                    a.AdSoyad,
                    a.Uzmanlik,
                    SporSalonuAdi = a.SporSalonu != null ? a.SporSalonu.Ad : null,
                    Musaitlikler = a.Musaitlikler
                        .Where(m => m.Gun == gun)
                        .Select(m => new
                        {
                            m.BaslangicSaati,
                            m.BitisSaati
                        }).ToList(),
                    // O tarihte randevusu olmayan antrenörleri filtrele
                    OGunRandevulari = a.Randevular
                        .Where(r => r.Baslangic.Date == tarih.Date && 
                                    r.Durum != RandevuDurumu.IptalEdildi)
                        .Select(r => new
                        {
                            r.Baslangic,
                            r.Bitis
                        }).ToList()
                })
                .ToListAsync();

            // Randevusu olmayan veya boş zamanı olan antrenörleri filtrele
            var gercektenUygunAntrenorler = new List<object>();
            
            foreach (var antrenor in uygunAntrenorler)
            {
                // Eğer o gün hiç randevusu yoksa, müsait
                if (!antrenor.OGunRandevulari.Any())
                {
                    gercektenUygunAntrenorler.Add(new
                    {
                        antrenor.Id,
                        antrenor.AdSoyad,
                        antrenor.Uzmanlik,
                        antrenor.SporSalonuAdi,
                        Musaitlikler = antrenor.Musaitlikler
                    });
                }
                else
                {
                    // Randevuları var ama müsaitlik saatleri içinde boş zaman var mı kontrol et
                    bool uygun = antrenor.Musaitlikler.Any(musaitlik =>
                    {
                        var musaitlikBaslangic = musaitlik.BaslangicSaati;
                        var musaitlikBitis = musaitlik.BitisSaati;
                        
                        // Tüm randevular müsaitlik saatlerinin dışında mı?
                        return antrenor.OGunRandevulari.All(randevu =>
                        {
                            var randevuBaslangic = randevu.Baslangic.TimeOfDay;
                            var randevuBitis = randevu.Bitis.TimeOfDay;
                            
                            // Randevu müsaitlik saatlerinin dışında mı?
                            return randevuBitis <= musaitlikBaslangic || randevuBaslangic >= musaitlikBitis;
                        });
                    });
                    
                    if (uygun)
                    {
                        gercektenUygunAntrenorler.Add(new
                        {
                            antrenor.Id,
                            antrenor.AdSoyad,
                            antrenor.Uzmanlik,
                            antrenor.SporSalonuAdi,
                            Musaitlikler = antrenor.Musaitlikler
                        });
                    }
                }
            }

            return Ok(gercektenUygunAntrenorler);
        }

        // GET: api/AntrenorlerApi/Uzmanlik/{uzmanlik}
        // Belirli bir uzmanlık alanına sahip antrenörleri getirme
        [HttpGet("Uzmanlik/{uzmanlik}")]
        public async Task<ActionResult<IEnumerable<object>>> GetAntrenorlerByUzmanlik(string uzmanlik)
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Where(a => a.Uzmanlik != null && a.Uzmanlik.Contains(uzmanlik))
                .Select(a => new
                {
                    a.Id,
                    a.AdSoyad,
                    a.Uzmanlik,
                    SporSalonuAdi = a.SporSalonu != null ? a.SporSalonu.Ad : null
                })
                .ToListAsync();

            return Ok(antrenorler);
        }
    }
}

