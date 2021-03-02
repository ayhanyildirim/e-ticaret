using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EticaretProje
{
    public class MyMail
    {
        public string ToMail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public MyMail(string _tomail, string _subject, string _body)
        {
            this.ToMail = _tomail;
            this.Subject = _subject;
            this.Body = _body;
        }
        public void SendMail()
        {

        }
    }
}