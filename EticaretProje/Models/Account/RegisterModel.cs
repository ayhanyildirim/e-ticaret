using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EticaretProje.Models.Account
{
    public class RegisterModel
    {
        public DB.Members Member { get; set; }
        public string rePassword { get; set; }
    }
}