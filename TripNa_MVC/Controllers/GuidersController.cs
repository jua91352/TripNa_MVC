﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TripNa_MVC.Models;

namespace TripNa_MVC.Controllers
{
    public class GuidersController : Controller
    {
        private readonly TripNaContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public GuidersController(TripNaContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        private readonly string _folder;
        private readonly static Dictionary<string, string> _contentTypes = new Dictionary<string, string>
        {
            {".jpg", "image/jpg"}
        };




        public IActionResult SignUp()
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            if (member == null)
            {
                return NotFound();
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("GuiderId,GuiderNickname,GuiderGender,GuiderArea,GuiderStartDate,GuiderIntro")] Guider guider, IFormFile guiderImage, IFormFile guiderVert)
        {
            if (ModelState.IsValid)
            {

                var memberEmail = HttpContext.Session.GetString("memberEmail");

                var memberContext = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

                var MemberId = _context.Members.FirstOrDefault(m => m.MemberId == memberContext.MemberId);

                if (memberContext != null && memberContext.GuiderId == null)
                {
                    // GuiderID 為空,可以註冊
                    ViewData["Message"] = "前往註冊";


                    _context.Add(guider);
                    await _context.SaveChangesAsync();
                    memberContext.GuiderId = guider.GuiderId;
                    await _context.SaveChangesAsync();

                    string guiderImageFileName = $"{guider.GuiderNickname}.jpg";
                    string guiderVertFileName = $"{guider.GuiderNickname}{guider.GuiderId}.jpg"; // 導遊證文件名


                    // 保存正面照片
                    if (guiderImage != null && guiderImage.Length > 0)
                    {
                        Console.WriteLine("111111111111111111111");
                        var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/大頭照/{guider.GuiderArea}", guiderImageFileName);
                        //using (var stream = new FileStream(imagePath, FileMode.Create))
                        //{
                        //    await guiderImage.CopyToAsync(stream);
                        //}
                        Console.WriteLine("2222222222222222222222");

                    }

                    // 保存導遊證
                    if (guiderVert != null && guiderVert.Length > 0)
                    {
                        var vertPath = Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/證照/{guider.GuiderArea}", guiderVertFileName);
                        //using (var stream = new FileStream(vertPath, FileMode.Create))
                        //{
                        //    await guiderVert.CopyToAsync(stream);
                        //}
                    }

                    return Redirect("/Guiders/GuiderCenter");

                }
                else
                {
                    // GuiderID 不為空,不能註冊,前往導遊會員中心
                    return Redirect("/Guiders/GuiderCenter");
                }

            }

            return View("home");
        }


        // GET: /Members/MemberCenter
        public IActionResult GuiderCenter()
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }
            var memberContext = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var MemberId = _context.Members.FirstOrDefault(m => m.MemberId == memberContext.MemberId);

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            if (member == null)
            {
                return NotFound();
            }


            var guidermember = from g in _context.Guiders
                               join m in _context.Members on g.GuiderId equals m.GuiderId into membersGroup
                               from m in membersGroup.DefaultIfEmpty()
                               where g.GuiderId == memberContext.GuiderId
                               select new
                               {
                                   Guider = g,
                                   Member = m
                               };

            //var guidermemberlist = guidermember.ToList();

            var guidermemberList = guidermember.Select(x => new guidermemberlist
            {
                Guider = x.Guider,
                Member = x.Member
            }).ToList();








            return View(guidermemberList);
        }














        //var model = new guidermemberlist
        //{

        //    guidermemberlists = guidermember.Select(o => new guidermemberlist
        //    {
        //        Guiders = new Guider
        //        {
        //            Guider = o.Guider
        //        },
        //        Members = new Member
        //        {
        //            Member = o.Member
        //        }

        //    }).ToList(),
        //    MemberId = MemberId
        //    GuiderId = g.
        //};





















        //[HttpPost]
        //public IActionResult GuiderCenter(Member updatedMember)
        //{
        //    var memberEmail = HttpContext.Session.GetString("memberEmail");
        //    if (string.IsNullOrEmpty(memberEmail))
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

        //    if (member == null)
        //    {
        //        return NotFound();
        //    }

        //    // 更新會員的資訊
        //    member.MemberName = updatedMember.MemberName;
        //    member.MemberPhone = updatedMember.MemberPhone;
        //    if (!string.IsNullOrEmpty(updatedMember.MemberPassword))
        //    {
        //        member.MemberPassword = updatedMember.MemberPassword;
        //    }

        //    _context.SaveChanges();
        //    return RedirectToAction("MemberCenter");
        //}






    }
}
