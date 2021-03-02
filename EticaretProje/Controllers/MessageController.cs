using EticaretProje.Filter;
using EticaretProje.Models.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EticaretProje.Controllers
{
    [MyAuthorize]
    public class MessageController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
           
            if (ısLogon() == false) return RedirectToAction("index", "Home");
            Models.Message.IndexModel model = new Models.Message.IndexModel();
            var currentId = CurrentUserId();
            model.Users = new List<SelectListItem>();

            var users = context.Members.Where(x => ((int)x.MemberType) > 0 && x.Id != currentId).ToList();
            model.Users = users.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = string.Format("{0} {1} ({2})", x.Name, x.Surname, x.MemberType.ToString())
            }).ToList(); 
          
            var mList = context.Messages
                .Where(x => x.ToMemberId == currentId || x.MessageReplies
                .Any(y => y.Member_Id == currentId))
                .ToList();

            model.Messages = mList;

            return View(model);
         
        }
        [HttpPost]
        public ActionResult SendMessage(Models.Message.SendMessageModel message)
        {
            if (ısLogon() == false) return RedirectToAction("index", "Home");

            DB.Messages mesaj = new DB.Messages()
            {
                Id = Guid.NewGuid(),
                AddedDate = DateTime.Now,
                IsRead = false,
                Subject = message.Subject,
                ToMemberId=message.ToUserId
               
            };
            var mRep = new DB.MessageReplies()
            {
                Id = Guid.NewGuid(),
                AddedDate = DateTime.Now,
                Member_Id = CurrentUserId(),
                Text = message.Messagebody
            };
            mesaj.MessageReplies.Add(mRep);
            context.Messages.Add(mesaj);
            context.SaveChanges();
            return RedirectToAction("Index", "Message");
        }

        [HttpGet]
        public ActionResult MessageReplies (string id)
        {
            if (ısLogon() == false) return RedirectToAction("index", "Home");
            var currentId = CurrentUserId();
            var guid = new Guid(id);
            DB.Messages message = context.Messages.FirstOrDefault(x => x.Id == guid);
            if (message.ToMemberId == currentId)
            {
                message.IsRead = true;
                context.SaveChanges();
            }


            MessageRepliesModel model = new MessageRepliesModel();
            

            model.MReplies = context.MessageReplies.Where(x => x.MessageId == guid).OrderBy(x=>x.AddedDate).ToList();

            return View(model);

        }

        [HttpPost]

        public ActionResult MessageReplies(DB.MessageReplies message)
        {
            if (ısLogon() == false) return RedirectToAction("index", "Home");
            message.AddedDate = DateTime.Now;
            message.Id = Guid.NewGuid();
            message.Member_Id = CurrentUserId();
            context.MessageReplies.Add(message);
            context.SaveChanges();

            return RedirectToAction("MessageReplies", "Message", new { id = message.MessageId });
        }
        [HttpGet]
        public ActionResult RenderMessage()
        {
            RenderMessageModel model = new RenderMessageModel();
            var currentId = CurrentUserId();
            var mList = context.Messages
                        .Where(x => x.ToMemberId == currentId || x.MessageReplies.Any(y => y.Member_Id == currentId))
                        .OrderByDescending(x => x.AddedDate);
            model.Messages = mList.Take(4).ToList();
            model.Count = mList.Count();

            return PartialView("_Message", model);
        }
        public ActionResult RemoveMessageReplies(string id)
        {
            var guid = new Guid(id);
            //mesja cepaları silindi
            var mReplies = context.MessageReplies.Where(x => x.MessageId == guid);
            context.MessageReplies.RemoveRange(mReplies);
            //mesajın kendisi silindi.
            var message = context.Messages.FirstOrDefault(x => x.Id == guid);
            context.Messages.Remove(message);

            context.SaveChanges();

            return RedirectToAction("Index", "Message");
        }
    }
}