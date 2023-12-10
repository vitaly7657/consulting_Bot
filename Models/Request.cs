using System;

namespace consulting_telegram_bot.Models
{
    public class Request
    {
        public int Id { get; set; }        
        public string? RequesterName { get; set; }
        public string? RequestEmail { get; set; }
        public string? RequestText { get; set; }
        public DateTime RequestTime { get; set; }
        public string? RequestStatus { get; set; }

    }
}
