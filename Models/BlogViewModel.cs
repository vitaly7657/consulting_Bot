using Microsoft.AspNetCore.Http;

namespace consulting_telegram_bot.Models
{
    public class BlogViewModel
    {
        public int? Id { get; set; }
        public string BlogTitle { get; set; }
        public string BlogDescription { get; set; }
        public IFormFile? PictureFile { get; set; }
    }
}
