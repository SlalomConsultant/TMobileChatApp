﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMobileChatApp.Models
{
    public class Chat
    {
        public int ChatId { get; set; }
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public string ChatText { get; set; }
        public DateTime PostDate { get; set; }
    }
}