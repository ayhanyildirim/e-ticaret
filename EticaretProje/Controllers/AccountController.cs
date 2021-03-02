using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using EticaretProje.Models.Account;
using System.Web.Services.Description;
using System.Threading;
using EticaretProje.Filter;

namespace EticaretProje.Controllers
{
    public class AccountController : BaseController
    {

        [HttpGet]
        public ActionResult Regıster()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Regıster(Models.Account.RegisterModel user)
        {
            try
            {

                if (user.rePassword != user.Member.Password)
                {
                    throw new Exception("Şifreler Aynı Değildir.");
                }
                if (context.Members.Any(x=>x.Email==user.Member.Email))
                {
                    throw new Exception("Bu e-mail kullanılmaktadır.");
                }
                user.Member.MemberType = DB.MemberTypess.Customer;
                user.Member.AddedDate = DateTime.Now;
                context.Members.Add(user.Member);
                context.SaveChanges();
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.reError = ex.Message;
                return View();
            }

        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Models.Account.LoginModel login)
        {
            try
            {

                var user = context.Members.FirstOrDefault(x => x.Password == login.Member.Password && x.Email == login.Member.Email);
                if (user != null)
                {
                    Session["LogonUser"] = user;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.reError = "Kullanıcı bilgileriniz yanlış veya hatalı!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.reError = ex.Message;
                return View();
            }

        }
        public ActionResult Logout()
        {
            Session["LogonUser"] = null;
            
            return RedirectToAction("Login","Account");
        }

        [HttpGet]
        public ActionResult Profil(int id = 0 ,string ad="")
        {
            List<DB.Addresses> addresses = null;
            DB.Addresses currentAddress = new DB.Addresses();
            if (id == 0)
            {
                id = base.CurrentUserId();
                addresses = context.Addresses.Where(x => x.Member_Id == id).ToList();
                if (!string.IsNullOrEmpty(ad))
                {
                    var guild = new Guid(ad);
                    currentAddress = context.Addresses.FirstOrDefault(x => x.Id == guild);
                }
            }
            var user = context.Members.FirstOrDefault(x => x.Id == id);
            if (user == null) return RedirectToAction("index", "Home");
            ProfileModel model = new ProfileModel()
            {
                Members = user,
                Addresses = addresses,
                CurrentAddress = currentAddress
            };
            return View(model);
        }
        [HttpGet]
        [MyAuthorize]
        public ActionResult ProfilEdit()
        {
            int id = base.CurrentUserId();
            var user = context.Members.FirstOrDefault(x => x.Id == id);
            if (user == null) return RedirectToAction("index", "home");
            ProfileModel model = new ProfileModel()
            {
                Members = user
            };
            return View(model);
        }
        [HttpPost]
        [MyAuthorize]
        public ActionResult ProfilEdit(ProfileModel model)
        {
            try
            {
                int id = CurrentUserId();
                var updateMember = context.Members.FirstOrDefault(x => x.Id == id);
                updateMember.ModifiedDate = DateTime.Now;
                updateMember.Bio = model.Members.Bio;
                updateMember.Name = model.Members.Name;
                updateMember.Surname = model.Members.Surname;
                if (string.IsNullOrEmpty(model.Members.Password)==false)
                {
                    updateMember.Password= model.Members.Password;
                }
                if (Request.Files!=null&&Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    if (file.ContentLength>0)
                    {
                        var folder = Server.MapPath("~/İmages/upload/");
                        var fileName = Guid.NewGuid() + ".jpg";
                        file.SaveAs(Path.Combine(folder, fileName));

                        var filePath = "İmages/upload/" + fileName;
                        updateMember.ProfileImageName = filePath;
                    }
                }
                context.SaveChanges();
                return RedirectToAction("Profil", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.Myerror = ex.Message;
                int id = CurrentUserId();
                var viewModel = new Models.Account.ProfileModel()
                {
                    Members = context.Members.FirstOrDefault(x => x.Id == id)
                };
                return View(viewModel);
                throw;
            }

           
        }
        [HttpPost]
        [MyAuthorize]
        public ActionResult Address(DB.Addresses address)
        {
            DB.Addresses _address = null;
            if (address.Id == Guid.Empty)
            {
                address.Id = Guid.NewGuid();
                address.AddedDate = DateTime.Now;
                address.Member_Id = base.CurrentUserId();
                context.Addresses.Add(address);
            }
            else
            {
                _address = context.Addresses.FirstOrDefault(x => x.Id == address.Id);
                _address.ModifiedDate = DateTime.Now;
                _address.Name = address.Name;
                _address.AdresDescription = address.AdresDescription;
            }
            context.SaveChanges();
            return RedirectToAction("Profil", "Account");
        }
        [HttpGet]
        [MyAuthorize]
        public ActionResult RemoveAddress(string id)
        {
            var guid = new Guid(id);
            var address = context.Addresses.FirstOrDefault(x => x.Id == guid);
            context.Addresses.Remove(address);
            context.SaveChanges();
            return RedirectToAction("Profil", "Account");
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var member = context.Members.FirstOrDefault(x => x.Email == email);
            if (member == null)
            {
                ViewBag.MyError = "Böyle bir hesap bulunamadı";
                return View();
            }
            else
            {
                var body = "Şifreniz : " + member.Password;
                MyMail mail = new MyMail(member.Email, "Şifremi Unuttum", body);
                mail.SendMail();
                TempData["Info"] = email + " mail adresinize şifreniz gönderilmiştir.";
                return RedirectToAction("Login");
            }

        }
    }
}

