using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessYounesApp.Data;
using FitnessYounesApp.Models;

namespace FitnessYounesApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RandevularApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RandevularApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RandevularApi
        // Tüm randevuları listeleme
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRandevular()
        {
            var randevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .Select(r => new
                {
                    r.Id,
                    AntrenorAdi = r.Antrenor != null ? r.Antrenor.AdSoyad : null,
                    HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : null,
                    UyeAdi = r.UyeProfil != null ? r.UyeProfil.AdSoyad : null,
                    r.Baslangic,
                    r.Bitis,
                    r.Ucret,
                    r.Durum
                })
                .ToListAsync();

            return Ok(randevular);
        }

        // GET: api/RandevularApi/Uye/{uyeId}
        // Belirli bir üyenin randevularını getirme
        [HttpGet("Uye/{uyeId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUyeRandevulari(int uyeId)
        {
            var randevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.UyeProfilId == uyeId)
                .Select(r => new
                {
                    r.Id,
                    AntrenorAdi = r.Antrenor != null ? r.Antrenor.AdSoyad : null,
                    HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : null,
                    r.Baslangic,
                    r.Bitis,
                    r.Ucret,
                    r.Durum
                })
                .OrderByDescending(r => r.Baslangic)
                .ToListAsync();

            return Ok(randevular);
        }

        // GET: api/RandevularApi/Tarih/{tarih}
        // Belirli bir tarihteki randevuları getirme
        [HttpGet("Tarih/{tarih}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRandevularByTarih(DateTime tarih)
        {
            var randevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .Where(r => r.Baslangic.Date == tarih.Date)
                .Select(r => new
                {
                    r.Id,
                    AntrenorAdi = r.Antrenor != null ? r.Antrenor.AdSoyad : null,
                    HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : null,
                    UyeAdi = r.UyeProfil != null ? r.UyeProfil.AdSoyad : null,
                    r.Baslangic,
                    r.Bitis,
                    r.Ucret,
                    r.Durum
                })
                .OrderBy(r => r.Baslangic)
                .ToListAsync();

            return Ok(randevular);
        }

        // GET: api/RandevularApi/Durum/{durum}
        // Belirli bir durumdaki randevuları getirme
        [HttpGet("Durum/{durum}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRandevularByDurum(RandevuDurumu durum)
        {
            var randevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.UyeProfil)
                .Where(r => r.Durum == durum)
                .Select(r => new
                {
                    r.Id,
                    AntrenorAdi = r.Antrenor != null ? r.Antrenor.AdSoyad : null,
                    HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : null,
                    UyeAdi = r.UyeProfil != null ? r.UyeProfil.AdSoyad : null,
                    r.Baslangic,
                    r.Bitis,
                    r.Ucret,
                    r.Durum
                })
                .OrderByDescending(r => r.Baslangic)
                .ToListAsync();

            return Ok(randevular);
        }
    }
}

