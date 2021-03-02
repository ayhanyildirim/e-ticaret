using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EticaretProje.Models.i
{
    public class CommentsModel
    {
        public List<DB.Comments> Comments { get; set; }
        public DB.Products Product { get; set; }
    }
}