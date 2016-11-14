﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using static System.Net.WebRequestMethods;
using System.Data.Entity;
using Let_s_Eat_Bee_Project.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace Let_s_Eat_Bee_Project.Controllers
{
    public class OrderController : Controller
    {

        ApplicationDbContext db = new ApplicationDbContext();
        // GET: Order
        public ActionResult Index(int page = 1)
        {
            IEnumerable<Order> orders = from u in db.Orders
                                        where u.CreationDateTime >= DateTime.Now
                                        orderby u.CreationDateTime
                                        select u;

            ViewBag.Page = page;
            return View(orders.ToList<Order>());
        }
        public ActionResult Create()
        {
            NewOrder order = new NewOrder();

            if (User.Identity.IsAuthenticated)
            {
                string id = User.Identity.GetUserId();
                IEnumerable<AuthorizedUser> u = db.AllUsers.OfType<AuthorizedUser>();
                AuthorizedUser user = u.FirstOrDefault<AuthorizedUser>(x => x.AppUserId == id);

                order.LastName = user.LastName;
                order.FirstName = user.FirstName;
            }
            return View(order);
        }
        [HttpPost]
        public ActionResult Create(NewOrder model)
        {
            Order order = new Order();
            AbstractUser user = null;
            if (User.Identity.IsAuthenticated)
            {
                string id = User.Identity.GetUserId();
                IEnumerable<AuthorizedUser> u = db.AllUsers.OfType<AuthorizedUser>();
                user = u.FirstOrDefault(x => x.AppUserId == id);
            }
            else
            {
                user = new UnauthorizedUser();
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                db.AllUsers.Add(user);
                db.SaveChanges();
            }
            order.Creator = user;
            order.CreatorId = user.Id;
            order.Address = model.Address;
            order.CreationDateTime = DateTime.Parse(model.Date + " " + model.Time);

            db.Orders.Add(order);
            db.SaveChanges();

            Joining join = new Joining();
            join.Order = order;
            join.OrderId = order.Id;
            join.Text = model.TextOfOrder;
            join.User = user;

            db.Joinings.Add(join);
            db.SaveChanges();
            user.Orders.Add(order);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                Order order = db.Orders.FirstOrDefault(x => x.Id == id);
                if (order != null)
                {
                    AuthorizedUser creator = (AuthorizedUser)order.Creator;
                    if (creator.AppUserId == null && creator.AppUserId != User.Identity.GetUserId())
                    {
                        return View("Index");
                    }
                    EditOrder currentOrder = new EditOrder();
                    currentOrder.Address = order.Address;
                    currentOrder.Date = order.CreationDateTime.Date.ToShortDateString();
                    currentOrder.Time = order.CreationDateTime.ToShortTimeString();
                    currentOrder.Text = order.Joinings.First().Text;
                    return View(currentOrder);
                }
            }
            return RedirectToAction("SignIn", "Account");

        }
        [HttpPost]
        public ActionResult Edit(EditOrder model, int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                Order order = db.Orders.FirstOrDefault(x => x.Id == id);
                if (order != null)
                {
                    order.Address = model.Address;
                    order.CreationDateTime = DateTime.Parse(model.Date + " " + model.Time);
                    order.Joinings.First().Text = model.Text;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Account");
        }
        public ActionResult Delete(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                Order order = db.Orders.FirstOrDefault(x => x.Id == id);
                if (order != null)
                {
                    db.Orders.Remove(order);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Account");
        }
        public ActionResult Detail(int id)
        {
            return View(db.Orders.Where(x=>x.Id==id).FirstOrDefault());
        }
    }
}