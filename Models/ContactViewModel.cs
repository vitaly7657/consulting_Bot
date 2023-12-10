using Microsoft.AspNetCore.Http;

namespace consulting_telegram_bot.Models
{
    public class ContactViewModel
    {
        public int? Id { get; set; }
        public string ContactText { get; set; }
        public string ContactLink { get; set; }
        public IFormFile? PictureFile { get; set; }
    }
}
