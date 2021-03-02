using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EticaretProje.Models.Message
{
    public class IndexModel
    {
        public List<System.Web.Mvc.SelectListItem> Users { get; set; }
        public List<DB.Messages> Messages { get; set; }
    }
}