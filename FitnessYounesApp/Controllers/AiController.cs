using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FitnessYounesApp.Controllers
{
    public class AiController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiController> _logger;

        public AiController(IConfiguration configuration, ILogger<AiController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetExerciseRecommendation([FromForm] ExerciseRecommendationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Boy) && string.IsNullOrWhiteSpace(request.Kilo) && 
                string.IsNullOrWhiteSpace(request.VucutTipi) && request.Fotograf == null)
            {
                ModelState.AddModelError("", "Lütfen en az bir bilgi girin (boy, kilo, vücut tipi veya fotoğraf).");
                return View("Index", request);
            }

            try
            {
                var recommendation = await GenerateExerciseRecommendation(request);
                ViewBag.Recommendation = recommendation;
                return View("Index", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI recommendation error");
                ModelState.AddModelError("", "Öneri oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                return View("Index", request);
            }
        }

        private async Task<string> GenerateExerciseRecommendation(ExerciseRecommendationRequest request)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            // Eğer API key yoksa, örnek bir öneri döndür
            if (string.IsNullOrEmpty(apiKey))
            {
                return GenerateMockRecommendation(request);
            }

            try
            {
                var prompt = BuildPrompt(request);
                
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "Sen bir fitness uzmanısın. Kullanıcılara kişiselleştirilmiş egzersiz ve diyet önerileri sunuyorsun." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 500,
                    temperature = 0.7
                };

                var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
                    return result?.choices?[0]?.message?.content ?? "Öneri oluşturulamadı.";
                }
                else
                {
                    _logger.LogWarning("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return GenerateMockRecommendation(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI API call failed");
                return GenerateMockRecommendation(request);
            }
        }

        private string BuildPrompt(ExerciseRecommendationRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Aşağıdaki bilgilere göre kişiselleştirilmiş bir egzersiz ve diyet planı öner:");
            
            if (!string.IsNullOrWhiteSpace(request.Boy))
                sb.AppendLine($"Boy: {request.Boy} cm");
            
            if (!string.IsNullOrWhiteSpace(request.Kilo))
                sb.AppendLine($"Kilo: {request.Kilo} kg");
            
            if (!string.IsNullOrWhiteSpace(request.VucutTipi))
                sb.AppendLine($"Vücut Tipi: {request.VucutTipi}");
            
            if (!string.IsNullOrWhiteSpace(request.Hedef))
                sb.AppendLine($"Hedef: {request.Hedef}");
            
            if (!string.IsNullOrWhiteSpace(request.EkstraBilgi))
                sb.AppendLine($"Ek Bilgiler: {request.EkstraBilgi}");

            sb.AppendLine("\nLütfen şunları içeren detaylı bir öneri sun:");
            sb.AppendLine("1. Haftalık egzersiz planı (günlük bazda)");
            sb.AppendLine("2. Önerilen egzersizler ve set/tekrar sayıları");
            sb.AppendLine("3. Beslenme önerileri");
            sb.AppendLine("4. Motivasyonel tavsiyeler");

            return sb.ToString();
        }

        private string GenerateMockRecommendation(ExerciseRecommendationRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== KİŞİSELLEŞTİRİLMİŞ EGZERSİZ VE DİYET PLANI ===\n");
            
            if (!string.IsNullOrWhiteSpace(request.Boy) && !string.IsNullOrWhiteSpace(request.Kilo))
            {
                sb.AppendLine($"Boy: {request.Boy} cm, Kilo: {request.Kilo} kg");
            }
            
            if (!string.IsNullOrWhiteSpace(request.VucutTipi))
            {
                sb.AppendLine($"Vücut Tipi: {request.VucutTipi}\n");
            }

            sb.AppendLine("📅 HAFTALIK EGZERSİZ PLANI:");
            sb.AppendLine("Pazartesi: Üst vücut antrenmanı (Göğüs, Sırt, Omuz)");
            sb.AppendLine("Salı: Kardiyovasküler egzersiz (30-45 dakika yürüyüş/koşu)");
            sb.AppendLine("Çarşamba: Alt vücut antrenmanı (Bacak, Kalça)");
            sb.AppendLine("Perşembe: Dinlenme veya hafif yoga/stretching");
            sb.AppendLine("Cuma: Tam vücut antrenmanı");
            sb.AppendLine("Cumartesi: Kardiyovasküler egzersiz");
            sb.AppendLine("Pazar: Dinlenme\n");

            sb.AppendLine("💪 ÖNERİLEN EGZERSİZLER:");
            sb.AppendLine("• Squat: 3 set x 12-15 tekrar");
            sb.AppendLine("• Push-up: 3 set x 10-12 tekrar");
            sb.AppendLine("• Plank: 3 set x 30-60 saniye");
            sb.AppendLine("• Lunges: 3 set x 12 tekrar (her bacak)");
            sb.AppendLine("• Dumbbell Rows: 3 set x 10-12 tekrar\n");

            sb.AppendLine("🥗 BESLENME ÖNERİLERİ:");
            sb.AppendLine("• Günde en az 2-3 litre su iç");
            sb.AppendLine("• Protein ağırlıklı beslen (tavuk, balık, yumurta, baklagiller)");
            sb.AppendLine("• Kompleks karbonhidratlar tüket (tam tahıllar, yulaf)");
            sb.AppendLine("• Sağlıklı yağlar ekle (zeytinyağı, avokado, kuruyemiş)");
            sb.AppendLine("• Günde 5-6 küçük öğün yemeye çalış\n");

            sb.AppendLine("💡 MOTİVASYONEL TAVSİYELER:");
            sb.AppendLine("• Her gün küçük adımlar at, büyük değişiklikler zaman alır");
            sb.AppendLine("• İlerlemeyi takip et ve kendini ödüllendir");
            sb.AppendLine("• Düzenli uyku ve stres yönetimi önemlidir");
            sb.AppendLine("• Sabırlı ol ve süreçten keyif almaya çalış\n");

            sb.AppendLine("Not: Bu öneriler genel bilgilendirme amaçlıdır. Kişiselleştirilmiş bir plan için bir antrenör veya diyetisyenle çalışmanız önerilir.");

            return sb.ToString();
        }
    }

    public class ExerciseRecommendationRequest
    {
        public string? Boy { get; set; }
        public string? Kilo { get; set; }
        public string? VucutTipi { get; set; }
        public string? Hedef { get; set; }
        public string? EkstraBilgi { get; set; }
        public IFormFile? Fotograf { get; set; }
    }

    public class OpenAIResponse
    {
        public List<Choice>? choices { get; set; }
    }

    public class Choice
    {
        public Message? message { get; set; }
    }

    public class Message
    {
        public string? content { get; set; }
    }
}

