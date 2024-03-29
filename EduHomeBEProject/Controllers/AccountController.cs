﻿using EduHomeBEProject.Models;
using EduHomeBEProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EduHomeBEProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM register)
        {
            if (!ModelState.IsValid)
                return View();
            AppUser user = new AppUser
            {
                Email = register.Email,
                UserName = register.Username,
                Fullname = register.Fullname
            };
            if (!register.Terms)
            {
                ModelState.AddModelError("Terms", "Don't agree with Terms & Condition ?");
                return View();
            }
            //bu result startupdaki sertler duzdu ya yox onu yoxlayir
            IdentityResult result = await _userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }

            await _userManager.AddToRoleAsync(user, "Member");

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string link = Url.Action(nameof(VerifyEmail), "Account", new { email = user.Email, token }, Request.Scheme, Request.Host.ToString());
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("mezemovrufet2020@gmail.com","Eduhome");
            mail.To.Add(new MailAddress(user.Email));
            mail.Subject = "Verify Email";
            string body = string.Empty;
            using (StreamReader reader = new StreamReader("wwwroot/assets/Template/verifyemail.html"))
            {
               body = reader.ReadToEnd();
            }
            string about = $"Welcome <strong>{user.Fullname}</strong> to our education home,please click the link in below to verif your account";
            body = body.Replace("{{link}}", link);
            mail.Body = body.Replace("{{About}}", about);
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("mezemovrufet2020@gmail.com", "mezemov15032000");
            smtp.Send(mail);

            TempData["Verify"] = true;

            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> VerifyEmail(string email,string token)
        {
            AppUser user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest();
            await _userManager.ConfirmEmailAsync(user,token);
            //verify olanda sign in olunmus formada qalmaq ucun
            await _signInManager.SignInAsync(user, true);
            TempData["Verified"] = true;
            return RedirectToAction("Index","Home");
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(AccountVM account)
        {
            AppUser user = await _userManager.FindByEmailAsync(account.AppUser.Email);
            if (user == null) return BadRequest();
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string link = Url.Action(nameof(ResetPassword), "Account", new { email = user.Email, token }, Request.Scheme, Request.Host.ToString());
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("mezemovrufet2020@gmail.com", "EduHome");
            mail.To.Add(new MailAddress(user.Email));
            mail.Subject = "Reset Password";
            mail.Body = $"<a href='{link}'> Please click here to reset your password </a>";
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("mezemovrufet2020@gmail.com", "mezemov15032000");
            smtp.Send(mail);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ResetPassword(string email, string token)
        {
            AppUser user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest();
            AccountVM model = new AccountVM
            {
                AppUser = user,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AccountVM account)
        {
            AppUser user = await _userManager.FindByEmailAsync(account.AppUser.Email);
            AccountVM model = new AccountVM
            {
                AppUser = user,
                Token = account.Token
            };
            if (!ModelState.IsValid) return View(model);
            IdentityResult result = await _userManager.ResetPasswordAsync(user, account.Token, account.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM login)
        {
            if (!ModelState.IsValid)
                return View();
            AppUser user = await _userManager.FindByNameAsync(login.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Username or Passord is wrong");
                return View();
            }
            //bu result user login ola bilir ya yox odu

            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user.UserName, login.Password, login.Remember, true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Your account has been blocked for 5 minutes due to multiple login attempts");
                    return View();
                }
                ModelState.AddModelError("", "Username or Password is wrong");
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            UserEditVM editedUser = new UserEditVM
            {
                Username = user.UserName,
                Email = user.Email,
                Fullname = user.Fullname
            };
            return View(editedUser);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditVM editedUser)
        {
            if (!ModelState.IsValid) return View(editedUser);
            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            UserEditVM eUser = new UserEditVM
            {
                Username = user.UserName,
                Email = user.Email,
                Fullname = user.Fullname
            };
            if (user.UserName != editedUser.Username && await _userManager.FindByNameAsync(editedUser.Username) != null)
            {
                ModelState.AddModelError("", $"{editedUser.Username} already existed");
                return View(eUser);
            }
            if (user.Email != editedUser.Email && await _userManager.FindByEmailAsync(editedUser.Email) != null)
            {
                ModelState.AddModelError("", $"{editedUser.Email} already taken");
                return View(eUser);
            }
            if (string.IsNullOrWhiteSpace(editedUser.CurrentPassword) && string.IsNullOrEmpty(editedUser.Password) && string.IsNullOrEmpty(editedUser.ComfirmPassword))
            {
                user.UserName = editedUser.Username;
                user.Email = editedUser.Email;
                user.Fullname = editedUser.Fullname;
                await _userManager.UpdateAsync(user);
                await _signInManager.SignInAsync(user, true);
            }
            else
            {
                if (editedUser.CurrentPassword == null || editedUser.Password == null || editedUser.ComfirmPassword == null)
                {
                    ModelState.AddModelError("", "Fill Password Requirment");
                    return View(eUser);
                }
                user.UserName = editedUser.Username;
                user.Email = editedUser.Email;
                user.Fullname = editedUser.Fullname;
                IdentityResult resultf = await _userManager.UpdateAsync(user);
                if (!resultf.Succeeded)
                {
                    foreach (IdentityError error in resultf.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(eUser);
                }
                IdentityResult result = await _userManager.ChangePasswordAsync(user, editedUser.CurrentPassword, editedUser.Password);
                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(eUser);
                }
                await _signInManager.SignInAsync(user, true);
            }
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Show()
        {
            return Content(User.Identity.Name.ToString());
        }
    }
}
