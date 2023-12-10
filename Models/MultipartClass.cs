using Microsoft.AspNetCore.Http;

namespace consulting_telegram_bot.Models
{
    public class MultipartClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IFormFile Pix { get; set; }
    }
}
