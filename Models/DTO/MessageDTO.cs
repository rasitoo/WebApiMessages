﻿namespace WebApiMessages.Models.DTO;

    public class MessageReadDTO
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
    public class MessageCreateDTO
    {
        public int ChatId { get; set; }
        public string Content { get; set; }
    }
    public class MessageUpdateDTO
    {
        public string Content { get; set; }
    }

