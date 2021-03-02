using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EticaretProje.Models.i
{
    public class ProductModel
    {
        public DB.Products Products { get; set; }
        public List<DB.Comments> Comments { get; set; }

    }
}