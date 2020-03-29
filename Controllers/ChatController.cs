using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using twothor.Models;
using Microsoft.EntityFrameworkCore;
using twothor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using twothor.Controllers;
using twothor.Services;

namespace twothor.Controllers
{
    public class ChatController : Controller
    {
        private EmailSender emailSender;
        private ApplicationDbContext _context; // databasen

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
            emailSender = new EmailSender();
        }


        [Authorize]
        public IActionResult Index() 
        {
            HomeController h = new HomeController(_context);
            ViewData["isTwoThor"] = h.getIsTwoThor(User.Identity.Name);
            ViewData["newMessages"] = h.getMessagesAmount(User.Identity.Name);
            ViewData["twoThorPicture"] = h.getProfilePicture(User.Identity.Name);

            var vm = GetChatLogList(User.Identity.Name).ToList(); // finner listen med den siste meldingen i chatter som brukeren er en del av. User.Identity.Name er emailen til brukeren
            
            
            return View(vm); // sender denne listen til viewet
        }
        
        [Authorize]
        public IActionResult ShowChat(string chosenName) //får inn et navn bassert på hvilken chat knapp man trykker på i Index viewet. Disse knappene har verdien til emailen til den man chatter med.
        {
            if (chosenName == null || !chosenName.Contains("@"))
            {
                return RedirectToAction("Index", "Home");
            }

            var vm = new ChatOverviewViewModel(); // lager en ny OverviewModel som skal sendes til viewet.
            vm.YourPPUrl = "null";
            vm.OtherPPUrl = "null";
            vm.messages = GetSpesificChatLog(User.Identity.Name, chosenName).ToList();  // fyller inn listen med tidligere meldinger i chatten fra databasen (funksjonen for dette er lengre nede i denne filen)
            vm.message =  new ChatMessage(User.Identity.Name, chosenName); // lager en midlertidig ny melding og fyller inn navnene til avsender og mottaker
            foreach(TwoThorProfile t in _context.DbTwoThorProfileList) // looper gjennom TwoThors for å finne ut om den som er logget på er en TwoThor
            {
                if(t.Email.Equals(chosenName))
                {
                    vm.OtherPPUrl = t.PictureUrl;
                }
                if (t.Email.Equals(User.Identity.Name)) // hvis han/hun er det:
                {
                    vm.YourPPUrl = t.PictureUrl;
                    vm.newJob = new TwoThorJob(User.Identity.Name, chosenName); // slå lages det en midlertidig ny jobb med de riktige navnene sendt inn

                    foreach(TwoThorSubjects s in _context.DbTwoThorSubjectList) // går gjennom alle fag som forsjellige 2Thor kan og macher mot navnet til brukeren.
                    {
                        if (s.TwoThorName.Equals(User.Identity.Name))
                        {
                            vm.newJob.SubjectsKnown.Add(new Subject(s.SubjectName)); // og legger dem til i listen over fag de kan i viewmodelen
                        }
                    }
                    if (vm.newJob.SubjectsKnown.Count == 0) // hvis de derimot ikke har noen fag registrert så slettes muligheten for å lage nye jobber. (dette løser f.eks. hvis du er 2thor i bare vårfag for da ville du ikke kunne starte en jobb i høstfag)
                    {
                        vm.newJob = null;
                    }
                }
            }
            foreach(TwoThorJob t in _context.DbTwoThorJobList) // går gjennom listen med jobber i databasen og macher opp med navnene. Hvis jobben er fullført men ikke gitt review på:
            {                   // false for in progress, true for done (database didnt like booleans)
                if (t.Completed.Equals("true") && t.Reviewed.Equals("false") && t.OtherName.Equals(User.Identity.Name) && chosenName != null && chosenName.Equals(t.TwoThorEmail))
                {
                    vm.newReview = new TwoThorReview(t.TwoThorEmail, t.JobSubject, t.OtherName); // så lages det en midlertidig review med navn og fag
                    break;
                }
            }
            _context.SaveChangesAsync();
            HomeController h = new HomeController(_context);
            ViewData["isTwoThor"] = h.getIsTwoThor(User.Identity.Name);
            ViewData["newMessages"] = h.getMessagesAmount(User.Identity.Name);
            ViewData["twoThorPicture"] = h.getProfilePicture(User.Identity.Name);
            return View(vm); // alt dette sendes til ShowChat viewet
        }

        [HttpPost]
        [Authorize]        
        public async Task<IActionResult> SendMessage(ChatMessage message) // blir kalt når man trykker send melding i showchat viewet (dvs _ChatMessagePartial)
        {
            if (message.RecipientName != null && message.Content != null && message.Content != "")
            {
                message.Name = User.Identity.Name; // navnet blir definitivt satt til brukerens navn (dette hindrer muligheter for juks i html formen)
                message.addTime(); // det blir lagt til et timestamp
                _context.DbChatLog.Add(message); // den nye meldingen blir lagt til i databasen
                await _context.SaveChangesAsync(); // databasen oppdateres
                return RedirectToAction("ShowChat", new {chosenName = message.RecipientName});
            }
            return RedirectToAction("ShowChat", new {chosenName = message.RecipientName});
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> NewJob(TwoThorJob job) // Blir kalt når en 2Thor trykker på ny jobb knappen i ShowChat (dvs _ChatNewJobPartial)
        {
            job.Completed = "false";
            job.Reviewed = "false";
            job.SubjectsKnown = new List<Subject>();
            job.TwoThorEmail = User.Identity.Name;
            if (job.OtherName != null && job.JobSubject != null && job.Date != DateTime.MinValue && job.Time != DateTime.MinValue && job.Location != null)
            {
                _context.DbTwoThorJobList.Add(job); // jobben blir lagt til i databasen
                ChatMessage message = new ChatMessage(job.TwoThorEmail, job.OtherName, 
                job.TwoThorEmail.Substring(0, job.TwoThorEmail.IndexOf("@"))  + " has scheduled an appointment in " + job.Location + " on " + job.Date.ToString().Substring(0,10) + " at "
                        + job.Time.ToString().Substring(11,2) +":"+ job.Time.ToString().Substring(14,2) + " for the subject " + job.JobSubject + "!"); // det sendes en melding til chatten med informasjonen om møtet
                message.addTime(); // meldingen blir timestampet
                _context.DbChatLog.Add(message); // meldingen lagres i databasen
                await _context.SaveChangesAsync();  // databasen oppdateres
                return RedirectToAction("ShowChat", new {chosenName = job.OtherName});
            }
            return RedirectToAction("ShowChat", new {chosenName = job.OtherName});
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> NewReview(TwoThorReview r) // blir kalt når en som har fått hjelp trykker på send review knappen i Showchat (dvs _ChatReviewPartial)
        {
            if(r.HammerAmount != 0) // sjekker om hammeren er null. dette er den hvis de ikke har endret noe 
            {
                foreach(TwoThorJob t in _context.DbTwoThorJobList) // går gjennom alle jobbene i databasen og matcher med navn
                {
                    if (t.Completed.Equals("true") && t.Reviewed.Equals("false") && r.OtherName.Equals(User.Identity.Name) && r.TwoThorUsername.Equals(t.TwoThorEmail))
                    {
                        _context.DbTwoThorJobList.Remove(t); // sletter jobben som var fullført
                        break;
                    }
                }
                if(r.Review == null)
                    r.Review = "";
                _context.DbTwoThorReviews.Add(r); // reviewet legges til i databasen
                await _context.SaveChangesAsync(); // databasen oppdateres
            }
            return RedirectToAction("ShowChat", new {chosenName = r.TwoThorUsername});
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }





        //The following is methods for working with the chat
        [Authorize]
        public List<ChatMessage> GetSpesificChatLog(string name, string otherName) // får inn navnene til de i chatten og skal finne dem i databasen
        {
            List<ChatMessage> temp = new List<ChatMessage>(); // lager en liste som kan returneres
            foreach (ChatMessage log in _context.DbChatLog) // går gjennom alle Chatmessage i databasen
            {
                if((log.Name.Equals(name) && log.RecipientName.Equals(otherName))
                || (log.Name.Equals(otherName) && log.RecipientName.Equals(name)))
                {
                    if(log.RecipientName.Equals(name))
                    {
                        log.read = true;
                    }
                    temp.Add(log); // legger dem til i meldingsloggen hvis navene matcher
                }
            }
            return temp; // returnerer listen til ShowChat funskjonen (som er den eneste funsjonen som kaller denne funsjonen)
        }

        [Authorize]
        public List<ChatDisplay> GetChatLogList(string name) // finner listen med chatter som den påloggede personen er med i
        {
            List<ChatDisplay> temp = new List<ChatDisplay>(); // lager en midlertidig list med ChatDisplay modeller (inneholder bare navn, tid og meldingsinnhold)

            foreach (ChatMessage log in _context.DbChatLog) // går gjennom alle Chatmessage i databasen
            {
                if (log.Name.Equals(name) || log.RecipientName.Equals(name)) // sjekker om personen er i chatten
                {
                    int n = checkExisting(log.Name, log.RecipientName, temp); // sjekker om chatten allerede er representert
                    if (n != -1)
                    {
                        temp.RemoveAt(n); // hvis den er det så fjerner den den forrige meldingen i listen
                    }

                    if (log.Name.Equals(name))
                    {
                        temp.Add(new ChatDisplay(log.RecipientName, log.Name, log.Content, log.Time, true)); // legger til den nye meldingen
                    }
                    else
                    {
                        temp.Add(new ChatDisplay(log.Name, log.Name, log.Content, log.Time, log.read)); // legger til den nye meldingen
                    }
                }
            }
            return temp; // returnerer listen til Index (som er den eneste funksjonen som kaller denne funksjonenen)
        }

        [Authorize]
        public int checkExisting(string name, string secondName, List<ChatDisplay> list) // sjekker om et navn allerede eksistere i en liste
        {
            int index = -1;
            foreach(ChatDisplay c in list)
            {
                index++;
                if(name.Equals(c.Chatname) || secondName.Equals(c.Chatname))
                    return index; // returnerer indexen i den midlertidige listen hvis den finner navnet
            }
            return -1; // returnerer -1 hvis navnet ikke finnes i listen
        }
    }
}
