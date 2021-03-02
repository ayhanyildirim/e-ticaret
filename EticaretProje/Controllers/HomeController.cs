using EticaretProje.DB;
using EticaretProje.Filter;
using EticaretProje.Models;
using EticaretProje.Models.i;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EticaretProje.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public ActionResult Index(int id = 0)
        {
            IQueryable<DB.Products> products = context.Products.OrderByDescending(x => x.AddedDate).Where(x => x.IsDeleted == false || x.IsDeleted == null);
            DB.Categories category = null;
            if (id > 0)
            {
                category = context.Categories.FirstOrDefault(x => x.Id == id);
                var allCategories = GetChildCategories(category);
                allCategories.Add(category);

                var catIntList = allCategories.Select(x => x.Id).ToList();
                //select * from Product where Category_Id in (1,2,3,4)
                products = products.Where(x => catIntList.Contains(x.Category_Id));
            }
            var viewModel = new Models.i.IndexModel()
            {
                Products = products.ToList(),
                Category = category
            };
            return View(viewModel);
        }
        [HttpGet]
        public ActionResult Product(int id = 0)
        {
            var pro = context.Products.FirstOrDefault(x => x.Id == id);

            if (pro == null) return RedirectToAction("Index", "Home");
         
            ProductModel model = new ProductModel()
            {
                Products = pro,
                Comments = pro.Comments.ToList()
                
            };
            return View(model);
        }
        [HttpPost]
        [MyAuthorize]
        public ActionResult Product(DB.Comments comment)
        {
            try
            {
                comment.Member_Id = base.CurrentUserId();
                comment.AddedDate = DateTime.Now;
                context.Comments.Add(comment);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.MyError = ex.Message;
            }
            return RedirectToAction("Product", "Home");
        }
        [HttpGet]
        public ActionResult AddBasket(int id , bool remove = false)
        {
            List<Models.i.Basketmodel> basket = null;
            if (Session["Basket"] == null)
            {
                basket = new List<Models.i.Basketmodel>();
            }
            else
            {
                basket = (List<Models.i.Basketmodel>)Session["Basket"];
            }
            if (basket.Any(x => x.Product.Id==id))
            {
                var pro = basket.FirstOrDefault(x => x.Product.Id == id);
                if (remove && pro.Count > 0)
                {
                    pro.Count -= 1;
                }
                else
                {
                    if (pro.Product.UnitsInStock > pro.Count)
                    {
                        pro.Count += 1;
                    }
                    else
                    {
                       TempData["MyError"] = "Yeterli Stok yok";
                    }
                }

            }
            else
            {
                var pro = context.Products.FirstOrDefault(x => x.Id ==id);
                if (pro != null && pro.IsContinued&&pro.UnitsInStock>0)
                {
                    basket.Add(new Models.i.Basketmodel()
                    {
                        Count = 1,
                        Product = pro

                    });
                }
                else if (pro!=null &&pro.IsContinued==false)
                {
                    TempData["MyError"] = "Ürün satışı durduruldu!";
                }
            }
            basket.RemoveAll(x=>x.Count<1);
            Session["Basket"] = basket;
            return RedirectToAction("Basket", "Home");
        }

        [HttpGet]
        public ActionResult Basket()
        {
            List<Models.i.Basketmodel> model = (List<Models.i.Basketmodel>)Session["Basket"];
         
            if (model==null)
            {
                model = new List<Models.i.Basketmodel>();
            }
            if (ısLogon())
            {
                int currentId = CurrentUserId();
                ViewBag.CurrentAddresses = context.Addresses
                                        .Where(x => x.Member_Id == currentId)
                                        .Select(x => new SelectListItem()
                                        {
                                           Text = x.Name,
                                           Value = x.Id.ToString()
                                        }).ToList();
            }
           
            ViewBag.TotalPrice = model.Select(x => x.Product.Price * x.Count).Sum();
            return View(model);
            
        }
        [HttpGet]
        public ActionResult RemoveBasket(int id)
        {
            List<Models.i.Basketmodel> basket = (List<Models.i.Basketmodel>)Session["Basket"];
            if (basket != null)
            {
                if (id > 0)
                {
                    basket.RemoveAll(x => x.Product.Id == id);
                }
                else if (id == 0)
                {
                    basket.Clear();
                }
                Session["Basket"] = basket;
            }
            return RedirectToAction("Basket", "Home");
        }

        [HttpPost]
        [MyAuthorize]
        public ActionResult Buy(string Address)
        {

            if (ısLogon())
            {
                try
                {
                    var basket = (List<Models.i.Basketmodel>)Session["Basket"];
                    var guid = new Guid(Address);
                    var _address = context.Addresses.FirstOrDefault(x => x.Id == guid);

                    //Sipariş Verildi = SV
                    //Ödeme Bildirimi = OB
                    //Ödeme Onaylandı = OO

                    var order = new DB.Orders()
                    {
                        AddedDate = DateTime.Now,
                        Address = _address.AdresDescription,
                        Member_Id = CurrentUserId(),
                        Status = "SV",
                        Id = Guid.NewGuid()
                    };
                    //5
                    //ahmet 5
                    //mehmet 5
                    foreach (Models.i.Basketmodel item in basket)
                    {
                        var oDetail = new DB.OrderDetails();
                        oDetail.AddedDate = DateTime.Now;
                        oDetail.Price = item.Product.Price * item.Count;
                        oDetail.Product_Id = item.Product.Id;
                        oDetail.Quantity = item.Count;
                        oDetail.Id = Guid.NewGuid();

                        order.OrderDetails.Add(oDetail);

                        var _product = context.Products.FirstOrDefault(x => x.Id == item.Product.Id);
                        if (_product != null && _product.UnitsInStock >= item.Count)
                        {
                            _product.UnitsInStock = _product.UnitsInStock - item.Count;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0} ürünü için yeterli stok yoktur veya silinmiş bir ürünü almaya çalışıyorsunuz.", item.Product.Name));
                        }
                    }
                    context.Orders.Add(order);
                    context.SaveChanges();
                    Session["Basket"] = null;
                }
                catch (Exception ex)
                {
                    TempData["MyError"] = ex.Message;
                }
                return RedirectToAction("Buy","Home");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        [HttpGet]
        [MyAuthorize]
        public ActionResult Buy()
        {
            if (ısLogon())
            {
                var currentId = CurrentUserId();
                IQueryable<DB.Orders> orders;
                if (((int)CurrentUser().MemberType) > 8)
                {
                    orders = context.Orders.Where(x => x.Status == "OB");
                }
                else
                {
                    orders = context.Orders.Where(x => x.Member_Id == currentId);
                }

                List<Models.i.BuyModel> model = new List<BuyModel>();
                foreach (var item in orders)
                {
                    var byModel = new BuyModel();
                    byModel.TotelPrice = item.OrderDetails.Sum(y => y.Price);
                    byModel.OrderName = string.Join(", ", item.OrderDetails.Select(y => y.Products.Name + "(" + y.Quantity + ")"));
                    byModel.OrderStatus = item.Status;
                    byModel.OrderId = item.Id.ToString();
                    byModel.Member = item.Members;
                    model.Add(byModel);
                }

                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        [MyAuthorize]
        public JsonResult OrderNotification(OrderNotificationModel model)
        {
            if (string.IsNullOrEmpty(model.OrderId) == false)
            {
                var guid = new Guid(model.OrderId);
                var order = context.Orders.FirstOrDefault(x => x.Id == guid);
                if (order != null)
                {
                    order.Description = model.OrderDescription;
                    order.Status = "OB";
                    context.SaveChanges();
                }
            }
            return Json("");
        }
        [HttpGet]
        //[HttpPost]
        public JsonResult GetProductDes(int id)
        {
            var pro = context.Products.FirstOrDefault(x => x.Id == id);
            return Json(pro.Description, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetOrder(string id)
        {
            var guid = new Guid(id);
            var order = context.Orders.FirstOrDefault(x => x.Id == guid);
            return Json(new
            {
                Description = order.Description,
                Address = order.Address
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [MyAuthorize]
        public JsonResult OrderCompilete(string id, string text)
        {
            var guid = new Guid(id);
            var order = context.Orders.FirstOrDefault(x => x.Id == guid);
            order.Description = text;
            order.Status = "OO";
            context.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }


    }
}