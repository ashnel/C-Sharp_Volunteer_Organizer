using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using C_Sharp_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using VolunteerPlanner.Models;

namespace C_Sharp_Project.Controllers
{
    public class DetailsController : Controller
    {
        private VolunteerContext _context;
        private string _controller;
        private string _action;

        public DetailsController(VolunteerContext context)
        {
            _context = context;
            _controller = "User";
            _action = "Login";
        }

        [HttpGet]
        [Route("Dashboard/{id}")]
        public IActionResult Dashboard(int id)
        {
            if(isLoggedIn()){
                setSessionViewData();
                GetEventInfo(id);
                GetUserInfo();
                ViewBag.AssignedVolunteers = AssignedVolunteerInfo(id);
                ViewBag.UnassignedVolunteers = UnassignedVolunteerInfo(id);
                return View();
            }else{
                return RedirectToAction(_action, _controller);
            }
        }

        private void setSessionViewData()
        {
            ViewData["Username"] = HttpContext.Session.GetString("UserName");
            ViewData["UserId"] = (int)HttpContext.Session.GetInt32("UserId");
        }

        public bool isLoggedIn(){
            int? UserId = HttpContext.Session.GetInt32("UserId");
            return (UserId != null);
        }

        public User GetUserInfo()
        {
            int UserId = (int)HttpContext.Session.GetInt32("UserId");
            User ConfirmedUser = _context.users.SingleOrDefault(User => User.UserId == UserId);
            ViewBag.ConfirmedUser = ConfirmedUser;
            return ConfirmedUser;
        }
        public Event GetEventInfo(int Id)
        {
            Event ConfirmedEvent = _context.events.Include(e => e.EventVolunteers).ThenInclude(u => u.User).Include(t => t.Tasks).ThenInclude(l => l.Loc).Include(ta => ta.Tasks).ThenInclude(v => v.TaskVolunteers).SingleOrDefault(ev => ev.EventId == Id);
            ViewBag.ConfirmedEvent = ConfirmedEvent;
            return ConfirmedEvent;
        }

        public List<User> AssignedVolunteerInfo(int Id)
        {
            Event ConfirmedEvent = GetEventInfo(Id);
            List<User> AssignedVolunteers = new List<User>();
            foreach(var task in ConfirmedEvent.Tasks)
            {
                foreach(var tvol in task.TaskVolunteers)
                {
                    AssignedVolunteers.Add(tvol.User);
                }
            }
            return AssignedVolunteers;
        }

        public List<User> UnassignedVolunteerInfo(int Id)
        {
            Event ConfirmedEvent = GetEventInfo(Id);
            List<User> AssignedVolunteers = AssignedVolunteerInfo(Id);
            List<User> UnassignedVolunteers = new List<User>();
            foreach(var evol in ConfirmedEvent.EventVolunteers)
            {
                UnassignedVolunteers.Add(evol.User);
                foreach(var person in AssignedVolunteers)
                {
                    System.Console.WriteLine("Assigned User: " + person.FirstName);
                    if(person.UserId == evol.UserId)
                    {
                        UnassignedVolunteers.Remove(evol.User);
                    }
                }
            }
            return UnassignedVolunteers;
        }
    }
}
