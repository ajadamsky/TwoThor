using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using twothor.Models;

using Microsoft.AspNetCore.Authorization;
using twothor.Data;

namespace twothor.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context; // databasen

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public string getMessagesAmount(string currentName)
        {
            HomePageModel vm = new HomePageModel();
            vm.NewMessages = 0;
            foreach(TwoThorProfile p in _context.DbTwoThorProfileList)
            {
                if(currentName.Equals(p.Email))
                {
                    vm.IsTwoThor = true;
                    break;
                }
            }
            foreach(ChatMessage m in _context.DbChatLog)
            {
                if(!m.read && m.RecipientName.Equals(currentName))
                {
                    vm.NewMessages++;
                }
            }

            return vm.NewMessages.ToString();
        }

        public string getIsTwoThor(string currentName)
        {
            HomePageModel vm = new HomePageModel();
            foreach(TwoThorProfile p in _context.DbTwoThorProfileList)
            {
                if(currentName.Equals(p.Email))
                {
                    vm.IsTwoThor = true;
                    break;
                }
            }
            if(vm.IsTwoThor)
            {
                return "True";
            }
            else
            {
                return "False";
            }
            
        }

        public string getProfilePicture(string name)
        {
            string returnValue = "null";
            foreach(TwoThorProfile p in _context.DbTwoThorProfileList)
            {
                if(name.Equals(p.Email))
                {
                    if(p.PictureUrl != null && !p.PictureUrl.Equals(""))
                        returnValue = p.PictureUrl;
                    break;
                }
            }
            return returnValue;
        }

        public IActionResult Index()
        {
            if(User.Identity.Name == null)
            {
                return RedirectToAction("Innsia","Home");
            }
            HomePageModel vm = new HomePageModel();
            vm.IsTwoThor = false;
            vm.NewMessages = 0;
            foreach(TwoThorProfile p in _context.DbTwoThorProfileList)
            {
                if(User.Identity.Name.Equals(p.Email))
                {
                    vm.IsTwoThor = true;
                    break;
                }
            }
            foreach(ChatMessage m in _context.DbChatLog)
            {
                if(!m.read && (m.Name.Equals(User.Identity.Name) || m.RecipientName.Equals(User.Identity.Name)))
                {
                    vm.NewMessages++;
                }
            }
            vm.SubjectList = new string[_context.DbSubjectList.Count()];
            int i = 0;
            foreach(Subject s in _context.DbSubjectList)
            {
                vm.SubjectList[i] = s.SubjectName.ToString();
                i++;
            }

            vm.ScrollLocation = "Nothing"; // to implement scrolling if wanted
            ViewData["isTwoThor"] = getIsTwoThor(User.Identity.Name);
            ViewData["newMessages"] = getMessagesAmount(User.Identity.Name);
            ViewData["twoThorPicture"] = getProfilePicture(User.Identity.Name);
            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Innsia()
        {
            if(User.Identity.Name != null)
            {
                return RedirectToAction("Index","Home");
            }
            return View();
        }

        [Authorize]
        public IActionResult SearchForSubject(string subject)
        {
             return RedirectToAction("Index", "Search", new {subject = subject}); 
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
