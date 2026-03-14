using ASP.NET.Assignment.PL.DTOs;
using ASP.NET.Assignment.PL.Helpers;
using ASP.NET.Assignment.PL.Helpers.Services.SMS;
using ASP.NET_Assignment.DAL.Models;
using AspNetCoreGeneratedDocument;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MimeKit;
using NuGet.Versioning;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace ASP.NET.Assignment.PL.Controllers
{
    public class UserController : Controller
    {
        public UserManager<AppUser> _userManager { get; }
        public SignInManager<AppUser> _signInManager { get; }
        public RoleManager<AppRole> _roleManager { get; }
        public RoleService _roleService { get; }

        public  ISMS _smsService { get;}

        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, RoleService roleService, ISMS smsService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _roleService = roleService;
            _smsService = smsService;
        }


        public async Task<IActionResult> Index(string? searchText)
        {
            
            var users = _userManager.Users.AsQueryable();
            if (!String.IsNullOrEmpty(searchText)) users = users.Where(U => U.UserName.ToLower().Contains(searchText.ToLower()));

                var usersToReturn = new List<UserToReturnDto>();
                foreach (var user in users)
                {
                    var Roles = await _userManager.GetRolesAsync(user);
                    usersToReturn.Add(new UserToReturnDto()
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Image = user.ImageName,
                        Roles = Roles
                    });
                }
            ViewBag.LoggedInUser = _userManager.GetUserId(User);
            return View(usersToReturn);
        }

        public async Task<IActionResult> Profile(string Id)
        {
            var user =await _userManager.FindByIdAsync(Id);
            var model = new UserToReturnDto() {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsConfirmed = user.PhoneNumberConfirmed,
                Image = user.ImageName
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(string id,UserToReturnDto model)
        {
            if (ModelState.IsValid)
            {
                var userr =await _userManager.GetUserAsync(User);
        
                if (model.ImageLink is not null && model.Image == "DefaultPFP.png")
                {
                    model.Image = AttachmentsSettings.Upload(model.ImageLink);
                }
                else if(model.ImageLink is null)
                {
                    return View(model);
                }
                else
                {
                    Console.WriteLine("hello");
                    AttachmentsSettings.Delete(userr.ImageName);
                    model.Image = AttachmentsSettings.Upload(model.ImageLink);
                }

              
                if (_userManager.Users.Where(u => u.PhoneNumber == model.PhoneNumber && u.Id != model.Id).Any(u=>u.PhoneNumber != null))
                {
                    ViewData["PhoneNumber"] = "This Number Is Already In Use";
                    return View(model);
                }
                
                var user = await _userManager.FindByIdAsync(model.Id);
                if(user.PhoneNumber != model.PhoneNumber)
                {
                    user.PhoneNumberConfirmed = false;
                }
                user.UserName = model.UserName;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.ImageName = model.Image;
                await _userManager.UpdateAsync(user);
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmPhoneNumber(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            Random rnd = new Random();
            var otp = rnd.Next(100000,999999).ToString();
            user.OTP = otp;
            await _userManager.UpdateAsync(user);
            SMSStructure Sms = new SMSStructure()
            {
                To = $"+2{user.PhoneNumber}",
                Body = $"Your OTP To Confirm Your Phonenumber is \n ${otp}"
            };
             var messageStat = _smsService.SendSMS(Sms);
            if (!messageStat)
            {
                return View("MessageFaildView");
            }
           
            ConfirmPhoneNumberDTO model = new ConfirmPhoneNumberDTO()
            {
                Phonenumber = user.PhoneNumber
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmPhoneNumber([FromBody]ConfirmPhoneNumberDTO dTO)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return BadRequest(new { message = "User not found" });

            if (user.OTP == dTO.otp)
            {
                user.PhoneNumberConfirmed = true;
                user.OTP = null;
                await _userManager.UpdateAsync(user);
                return Ok(new { success = true, message = "OTP confirmed!" });
            }

            return BadRequest(new { success = false, message = "Invalid OTP" });
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id is null) return BadRequest("Invalid Id");

            var user = _userManager.FindByIdAsync(id).Result;
            if(user is null) return BadRequest("User not Found!");
            var currentUseriD = _userManager.GetUserId(User);
            if (currentUseriD is null) return View("Error");

            var currentUser = await _userManager.FindByIdAsync(currentUseriD);
            if (currentUser is null) return View("Error");

            var CanEdit = await _roleService.CanEditUserAsync(currentUser, user);
            TempData["CanEdit"] = CanEdit;

            var dto = new UserToReturnDto()
            {
                Id = user.Id,
                UserName = user.UserName, 
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = _userManager.GetRolesAsync(user).Result
            };
            ViewBag.Id = user.Id;
            return View(dto);
        }
        [HttpGet]
        public IActionResult Delete()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string? id)
        {
            var userToDelete = await _userManager.FindByIdAsync(id);
            if (userToDelete is null)
            {
                return View("Models/DeletionUnSuccess");
            }
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser is null) return View("Error");

            var currentUserRoles =await _userManager.GetRolesAsync(currentUser);
            var currentUserMaxRole =await _roleService.GetHighetRoleLevelAsync(currentUserRoles);

            var userToDeleteRoles = await _userManager.GetRolesAsync(userToDelete);
            var userToDeleteMaxRoles = await _roleService.GetHighetRoleLevelAsync(userToDeleteRoles);

            if(currentUserMaxRole > userToDeleteMaxRoles)
            {
                if (currentUserId == userToDelete.Id)
                {

                    var deletedRes = await _userManager.DeleteAsync(userToDelete);
                    if (deletedRes.Succeeded)
                    {
                        await _signInManager.SignOutAsync();
                        return RedirectToAction(nameof(SignIn), "Account");
                    }
                    return View("Models/DeletionUnSuccess");

                }
                var res = await _userManager.DeleteAsync(userToDelete);


                if (res.Succeeded)
                {
                    return View("Models/DeletionSuccess");
                }
            }
            return View("Models/DeletionUnSuccess");
        }

        [HttpGet]
        public async Task<IActionResult> AddOrRemoveRole(string? userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user is null) return View("Error");

            var userRoles =await _userManager.GetRolesAsync(user);
            var AllRoles =await _roleManager.Roles.ToListAsync();
            var CurrentUser = await _userManager.GetUserAsync(User);
            var CurrentUserRole = await _userManager.GetRolesAsync(CurrentUser);

            ViewBag.CurrentUserRole = AllRoles.Where(R => CurrentUserRole.Contains(R.Name)).Select(R=>R.Level).Max();

            var model = new EditUserRolesViewModel()
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = AllRoles.Select(R => new UserRolesDto()
                {
                    RoleId = R.Id,
                    RoleName = R.Name,
                    Level = R.Level,
                    IsSelected = userRoles.Contains(R.Name)
                }).ToList()
                
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddOrRemoveRole(EditUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user is null) return View("Error");

            var CurrentRoles = await _userManager.GetRolesAsync(user);
            var SelectedRoles = model.Roles.Where(R => R.IsSelected).Select(R => R.RoleName);

            await _userManager.AddToRolesAsync(user, SelectedRoles.Except(CurrentRoles));

            await _userManager.RemoveFromRolesAsync(user, CurrentRoles.Except(SelectedRoles));

            return RedirectToAction(nameof(Index));
        }
    }
}
    