using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using twothor.Data;
using twothor.Models;
using twothor.Models.AccountViewModels;
using twothor.Services;
using Novell.Directory.Ldap;

namespace twothor.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public IActionResult Profile(string chosenName) 
        {
            HomeController h = new HomeController(_context);
            ViewData["isTwoThor"] = h.getIsTwoThor(User.Identity.Name);
            ViewData["newMessages"] = h.getMessagesAmount(User.Identity.Name);
            ViewData["twoThorPicture"] = h.getProfilePicture(User.Identity.Name);

            var vm = new TwoThorOverviewViewModel();
            vm.ownProfile = false;
            
            if (chosenName == null)
            {
                chosenName = User.Identity.Name;
            }
            foreach(TwoThorProfile t in _context.DbTwoThorProfileList)
            {
                if (chosenName.Equals(t.Email))
                {
                    vm.Profile = t;
                    break;
                }
            }
            if(vm.Profile == null)
            {
                return RedirectToAction("Index", "Home");
            }
            vm.Subjects = new List<TwoThorSubjects>(); 
            foreach(TwoThorSubjects s in _context.DbTwoThorSubjectList)
            {
                if(vm.Profile.Email.Equals(s.TwoThorName))
                    vm.Subjects.Add(s);
            } 

            if(User.Identity.Name == vm.Profile.Email) // you chose your own profile
            {
                vm.ownProfile = true;
                vm.RegisterModel = new RegisterViewModel();
                vm.RegisterModel.Name = vm.Profile.Name;
                vm.RegisterModel.Description = vm.Profile.Description;
                vm.RegisterModel.Email = vm.Profile.Email;
                vm.RegisterModel.Subjects = _context.DbSubjectList.ToList();
                vm.RegisterModel.SubjectsKnown = new string[_context.DbSubjectList.Count()];
                vm.Jobs = new List<TwoThorJob>();
                for(int i = 0; i < vm.Subjects.Count(); i++)
                {
                    vm.RegisterModel.SubjectsKnown[i] = vm.Subjects.ElementAt(i).SubjectName;
                }
                foreach(TwoThorJob j in _context.DbTwoThorJobList)
                {
                    if(j.TwoThorEmail == vm.Profile.Email && j.Completed.Equals("false"))
                    {
                        vm.Jobs.Add(j);
                    }
                }            
            }
            else
            {
                vm.Message = new ChatMessage(User.Identity.Name, vm.Profile.Email);
            }

            vm.Reviews = new List<TwoThorReview>();
            foreach(TwoThorReview j in _context.DbTwoThorReviews)
            {
                if(j.TwoThorUsername == vm.Profile.Email)
                {
                    vm.Reviews.Add(j);
                }
            } 

            return View(vm);
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [Authorize]
        public IActionResult SearchForSubject(string subject)
        {
             return RedirectToAction("Index", "Search", new {subject = subject}); 
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if(User.Identity.Name == null)
            {
                LoginViewModel vm = new LoginViewModel();
                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                ViewData["ReturnUrl"] = returnUrl;
                return View(vm);
            }
            return RedirectToLocal("LogoutNow");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginToDebug (string user)
        {
            if(!user.Equals(""))
            {
                 var result = await _signInManager.PasswordSignInAsync(user, "Password1.", false, lockoutOnFailure: false);
                 if(result.Succeeded)
                 {
                     return RedirectToAction("Index", "Home");
                 }
            }
            return RedirectToLocal("LogoutNow");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginFunction(LoginViewModel model)
        {
            if (ModelState.IsValid && model.Email != null && model.Password != null)
            {
                LdapEntry checkUser = null;
                if(model.Email.Contains("@"))
                {
                    checkUser = Connect(model.Email.Substring(0, model.Email.IndexOf("@")), model.Password);
                }
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
                if(checkUser != null && !result.Succeeded && notAlreadyInUserBase(model))
                {
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                    var newUser = await _userManager.CreateAsync(user, model.Password);
                    if (newUser.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created a new account with password.");
                        return RedirectToAction("Index","Home");
                    }
                }
                else if (model.Email == "admin@student.uia.no")
                {
                    result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
                    if(result.Succeeded)
                    {
                        //Just in case we want something spessial to happen
                    }
                }
                else if(result.Succeeded)
                {
                    return RedirectToAction("Index","Home");
                }
                return RedirectToLocal("LogoutNow");
            }
            
            // If we got this far, something failed, redisplay form
            return RedirectToAction("Login","Account");
        }

        private bool notAlreadyInUserBase(LoginViewModel model)
        {
            foreach(ApplicationUser name in _userManager.Users)
            {
                if(name.Email.Equals(model.Email))
                {
                    return false;
                }
            }
            return true;
        }

        public LdapEntry Connect(string name, string pass)
        {
            // Try to connect with the given credentials.
            ILdapConnection connection;
            try{
                connection = GetConnection(name, pass);
            }
            catch{
                return null;
            }

            // Debug this variable to check if the returned attributes are correct.
            // You can search for any username on ldap.uia.no, as long you has a valid connection.
            LdapEntry user;
            try{
                user = SearchForUser(connection, name);
            }
            catch{
                return null;
            }
            return user;
        }

        // Connect to ldap.uia.no with the given credentials
        ILdapConnection GetConnection(string username, string password)
        {
            // Creating an LdapConnection instance 
            LdapConnection ldapConn = new LdapConnection() { SecureSocketLayer = true };

            // Connect function will create a socket connection to the server - Port 389 for insecure and 3269 for secure.
            // These two ports won't work for ldap.uia.no. Use port 636 instead, which is the port for ldap over TLS/SSL.
            ldapConn.Connect("ldap.uia.no", 636);

            // Bind function with null user dn and password value will perform anonymous bind to the given LDAP server.
            // This is not possible with dlap.uia.no. You must bind with valid credentials to access it.
            ldapConn.Bind(@"uid=" + username + ",cn=users,cn=system,dc=uia,dc=no", password);

            return ldapConn;
        }
        
        // Search and retrieve an user's information
        LdapEntry SearchForUser(ILdapConnection connection, string username)
        {
            // Prepare a search filter with the given username
            string filter = $"(&(objectClass=posixAccount)(uid=" + username + "))";

            // Search with the filter 
            var search = connection.Search("cn=users,cn=system,dc=uia,dc=no", LdapConnection.SCOPE_SUB, filter, null, false);

            // Retrieve result from search
            if (search.hasMore())
            {
                var userObj = search.next();

                return userObj;
            }

            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpPost]
        [Authorize]        
        public async Task<IActionResult> SendMessage(ChatMessage message) // blir kalt når man trykker send melding i showchat viewet (dvs _ChatMessagePartial)
        {
            if (message.Content != null && message.Content != "")
            {
                message.addTime(); // det blir lagt til et timestamp
                message.Name = User.Identity.Name; // navnet blir definitivt satt til brukerens navn (dette hindrer muligheter for juks i html formen)
                _context.DbChatLog.Add(message); // den nye meldingen blir lagt til i databasen
                await _context.SaveChangesAsync(); // databasen oppdateres
                return RedirectToAction("ShowChat", "Chat", new {chosenName = message.RecipientName});
            }
            return RedirectToAction("ShowChat", "Chat", new {chosenName = message.RecipientName});
        }

        [HttpGet]
        [Authorize]
        public IActionResult Register(string returnUrl = null)
        {
            HomeController h = new HomeController(_context);
            ViewData["isTwoThor"] = h.getIsTwoThor(User.Identity.Name);
            ViewData["newMessages"] = h.getMessagesAmount(User.Identity.Name);
            ViewData["twoThorPicture"] = h.getProfilePicture(User.Identity.Name);

            ViewData["ReturnUrl"] = returnUrl;
            foreach(TwoThorProfile t in _context.DbTwoThorProfileList)
            {
                if (t.Email.Equals(User.Identity.Name))
                {
                    return RedirectToAction("Profile", new {chosenName = User.Identity.Name}); 
                }
            }
            RegisterViewModel vm = new RegisterViewModel();
            vm.Subjects = _context.DbSubjectList.ToList();
            vm.SubjectsKnown = new string[_context.DbSubjectList.Count()];
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            model.Email = User.Identity.Name;
            if (model != null && model.Name != null && model.Description != null)
            {
                var newTwoThor = new TwoThorProfile(model.Name, model.Email, model.Description);
                newTwoThor.PictureUrl = "null";

                foreach(string name in model.SubjectsKnown)
                {
                    foreach(Subject s in _context.DbSubjectList)
                    {
                        if(name.Length > 6 && s.SubjectName.Length > 6 && name.Substring(0,6).Equals(s.SubjectName.Substring(0,6)))
                            _context.DbTwoThorSubjectList.Add(new TwoThorSubjects(model.Email, name));
                    }
                }

                _context.DbTwoThorProfileList.Add(newTwoThor);      
                await _context.SaveChangesAsync();             
                return RedirectToAction("Profile", new {chosenName = model.Email});
            }

            // If we got this far, something failed, redisplay form
            model.Subjects = _context.DbSubjectList.ToList();
            model.SubjectsKnown = new string[_context.DbSubjectList.Count()];
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (model != null && model.Name != null && model.Description != null)
            {
                model.Email = User.Identity.Name;
                TwoThorProfile TwoThor = null;
                foreach(TwoThorProfile t in _context.DbTwoThorProfileList) // looper gjennom TwoThors for å finne ut om den som er logget på er en TwoThor
                {
                    if (t.Email.Equals(User.Identity.Name)) // hvis han/hun er det:
                    {
                        TwoThor = t;
                        break;
                    }
                }
                if(TwoThor != null)
                {
                    TwoThor.Name = model.Name;
                    TwoThor.Description = model.Description;

                    foreach(TwoThorSubjects s in _context.DbTwoThorSubjectList)
                    {
                        if(model.Email.Equals(s.TwoThorName))
                            _context.DbTwoThorSubjectList.Remove(s);
                    }
                    foreach(string name in model.SubjectsKnown)
                    {
                        foreach(Subject s in _context.DbSubjectList)
                        {
                            if(name.Length > 6 && s.SubjectName.Length > 6 && name.Substring(0,6).Equals(s.SubjectName.Substring(0,6)))
                                _context.DbTwoThorSubjectList.Add(new TwoThorSubjects(model.Email, name));
                        }
                    }    
                    await _context.SaveChangesAsync();             
                    return RedirectToAction("Profile", new {chosenName = model.Email});
                }                
            }

            // If we got this far, something failed, send to front page
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfilePicture(string Url, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (Url != null && Url != "")
            {
                TwoThorProfile TwoThor = null;
                foreach(TwoThorProfile t in _context.DbTwoThorProfileList) // looper gjennom TwoThors for å finne ut om den som er logget på er en TwoThor
                {
                    if (t.Email.Equals(User.Identity.Name)) // hvis han/hun er det:
                    {
                        TwoThor = t;
                        break;
                    }
                }
                if(TwoThor != null)
                {
                    TwoThor.PictureUrl = Url;

                    await _context.SaveChangesAsync();             
                    return RedirectToAction("Profile", new {chosenName = User.Identity.Name});
                }                
            }

            // If we got this far, something failed, send to front page
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(TwoThorJob job, string Submit, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (job != null && Submit != null)
            {
                TwoThorJob tjob = null;
                job.TwoThorEmail = User.Identity.Name;
                foreach(TwoThorJob t in _context.DbTwoThorJobList) // looper gjennom TwoThorsjobs for å finne jobben
                {
                    if (t.TwoThorEmail.Equals(User.Identity.Name) && t.OtherName.Equals(job.OtherName)) // hvis han/hun er det:
                    {
                        tjob = t;
                        break;
                    }
                }
                if(tjob.Completed.Equals("false") && tjob.Reviewed.Equals("false"))
                {
                    if ( tjob != null && Submit.Equals("Delete"))
                    {
                        _context.DbChatLog.Add(new ChatMessage(tjob.TwoThorEmail, tjob.OtherName,
                        tjob.TwoThorEmail.Substring(0, tjob.TwoThorEmail.IndexOf("@")) + " has canceled/deleted your appointment on " + tjob.Date.ToString().Substring(0,10) + " at " + tjob.Time.ToString().Substring(11,2)+":"+tjob.Time.ToString().Substring(14,2) + "!"));
                        _context.DbTwoThorJobList.Remove(tjob);
                        await _context.SaveChangesAsync();                     
                    }
                    else if (tjob != null && Submit.Equals("Complete"))
                    {
                        tjob.Completed = "true";
                        tjob.Reviewed = "false";
                        _context.DbChatLog.Add(new ChatMessage(tjob.TwoThorEmail, tjob.OtherName, 
                        tjob.TwoThorEmail.Substring(0, tjob.TwoThorEmail.IndexOf("@")) + " has asked for a review for helping you with " + tjob.JobSubject));
                        await _context.SaveChangesAsync();                     
                    }
                }
                return RedirectToAction("Profile", new {chosenName = User.Identity.Name});              
            }

            // If we got this far, something failed, send to front page
            return RedirectToAction("Profile", "Account");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public async Task<IActionResult> LogoutNow()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
