using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers
{
    [Authorize(Roles = "Admin,Analyst")]
    public class PolicyController : Controller
    {
        private readonly VRTrackingAppContext _db;

        public PolicyController(VRTrackingAppContext db)
        {
            _db = db;
        }

        // GET: Policy
        public async Task<IActionResult> Index(string? title, string? status)
        {
            ViewData["Title"] = "Policies";
            ViewBag.Title = title;
            ViewBag.Status = status;

            var query = _db.Policies.AsNoTracking();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(p => p.Title.Contains(title));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var policies = await query.ToListAsync();
            return View(policies);
        }

        // GET: Policy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var policy = await _db.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (policy == null)
            {
                return NotFound();
            }

            return View(policy);
        }

        // GET: Policy/Create
        public IActionResult Create()
        {
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName");
            return View();
        }

        // POST: Policy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id","Title","Description","Category","Version","EffectiveDate","ReviewDate","OwnerUserId","Status")] Policy policy)
        {
            if (ModelState.IsValid)
            {
                policy.CreatedAt = DateTime.UtcNow;
                policy.UpdatedAt = DateTime.UtcNow;
                _db.Add(policy);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", policy.OwnerUserId);
            return View(policy);
        }

        // GET: Policy/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var policy = await _db.Policies.FindAsync(id);
            if (policy == null)
            {
                return NotFound();
            }
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", policy.OwnerUserId);
            return View(policy);
        }

        // POST: Policy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id","Title","Description","Category","Version","EffectiveDate","ReviewDate","OwnerUserId","Status")] Policy policy)
        {
            if (id != policy.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    policy.UpdatedAt = DateTime.UtcNow;
                    _db.Update(policy);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PolicyExists(policy.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", policy.OwnerUserId);
            return View(policy);
        }

        // GET: Policy/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var policy = await _db.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (policy == null)
            {
                return NotFound();
            }

            return View(policy);
        }

        // POST: Policy/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var policy = await _db.Policies.FindAsync(id);
            if (policy != null)
            {
                _db.Policies.Remove(policy);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PolicyExists(int id)
        {
            return _db.Policies.Any(e => e.Id == id);
        }
    }
}
