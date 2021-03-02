using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EticaretProje.Models.Account
{
    public class ProfileModel
    {
        public DB.Members Members { get; set; }
        public List<DB.Addresses> Addresses { get; set; }
        public DB.Addresses CurrentAddress { get; set; }
    }
}