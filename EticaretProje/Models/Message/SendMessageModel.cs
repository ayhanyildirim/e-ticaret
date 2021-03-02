using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EticaretProje.Models.Message
{
    public class SendMessageModel
    {
        public string Subject { get; set; }
        public string Messagebody { get; set; }
        public int ToUserId { get; set; }
    }
}