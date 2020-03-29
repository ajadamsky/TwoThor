using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using twothor.Data;
using twothor.Models;

namespace twothor.Controllers
{
    public class SearchController : Controller
    {
        
        private ApplicationDbContext _context; // databasen

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult VisitProfile(string chosenName)
        {
            if(chosenName == null)
            {
                return RedirectToAction("Index", "Home");
            }
            foreach(TwoThorProfile p in _context.DbTwoThorProfileList)
            {
                if(chosenName.Equals(p.Email))
                {
                    return RedirectToAction("Profile", "Account", new {chosenName = chosenName}); 
                }
            }
            
            // if something went wrong
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult SearchForSubject(string subject)
        {
             return RedirectToAction("Index", "Search", new {subject = subject}); 
        }

        [Authorize]
        public IActionResult Index(string subject)
        {
            HomeController h = new HomeController(_context);
            ViewData["isTwoThor"] = h.getIsTwoThor(User.Identity.Name);
            ViewData["newMessages"] = h.getMessagesAmount(User.Identity.Name);
            ViewData["twoThorPicture"] = h.getProfilePicture(User.Identity.Name);

            bool found = false;
            SearchResultModel vm = new SearchResultModel();
            if (subject == null)
            {
                // if something went wrong
                return RedirectToAction("Index", "Home");
            }
            foreach(Subject s in _context.DbSubjectList)
            {
                if(subject.Length >= 6 && s.SubjectName.Substring(0,6).Equals(subject.Substring(0,6), StringComparison.InvariantCultureIgnoreCase))
                {
                    found = true;
                    vm.SubjectName = s.SubjectName;
                    vm.Profiles = new List<TwoThorProfile>();
                    vm.Reviews = new List<TwoThorReview>();
                    break;
                }
            }
            
            if(found)
            {
                foreach(TwoThorSubjects s in _context.DbTwoThorSubjectList)
                {
                    if (s.SubjectName.Substring(0,6).Equals(vm.SubjectName.Substring(0,6), StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach(TwoThorProfile p in _context.DbTwoThorProfileList)
                        {
                            if(s.TwoThorName.Equals(p.Email, StringComparison.InvariantCultureIgnoreCase))
                            {
                                vm.Profiles.Add(p);
                                break;
                            }
                        }
                    }
                }
                if(vm.Profiles.Count != 0)
                {
                    foreach(TwoThorReview r in _context.DbTwoThorReviews)
                    {
                        foreach(TwoThorProfile p in vm.Profiles)
                        {
                            if(r.TwoThorUsername.Equals(p.Email, StringComparison.InvariantCultureIgnoreCase) && r.SubjectName.Equals(vm.SubjectName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                vm.Reviews.Add(r);
                            }
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
                return View(vm);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
