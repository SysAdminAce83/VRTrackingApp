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
    public class StandardController : Controller
    {
        private readonly VRTrackingAppContext _db;

        public StandardController(VRTrackingAppContext db)
        {
            _db = db;
        }

        // GET: Standard
        public async Task<IActionResult> Index(string? title, string? status)
        {
            ViewData["Title"] = "Standards";
            ViewBag.Title = title;
            ViewBag.Status = status;

            var query = _db.Standards
                .Include(s => s.Policy)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(s => s.Title.Contains(title));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            var standards = await query.ToListAsync();
            return View(standards);
        }

        // GET: Standard/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _db.Standards
                .Include(s => s.Policy)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (standard == null)
            {
                return NotFound();
            }

            return View(standard);
        }

        // GET: Standard/Create
        public IActionResult Create()
        {
            ViewBag.PolicyId = new SelectList(_db.Policies.OrderBy(p => p.Title), "Id", "Title");
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName");
            return View();
        }

        // POST: Standard/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id","PolicyId","Title","Description","Version","EffectiveDate","ReviewDate","OwnerUserId","Status")] Standard standard)
        {
            if (ModelState.IsValid)
            {
                standard.CreatedAt = DateTime.UtcNow;
                standard.UpdatedAt = DateTime.UtcNow;
                _db.Add(standard);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PolicyId = new SelectList(_db.Policies.OrderBy(p => p.Title), "Id", "Title", standard.PolicyId);
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", standard.OwnerUserId);
            return View(standard);
        }

        // GET: Standard/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _db.Standards.FindAsync(id);
            if (standard == null)
            {
                return NotFound();
            }
            ViewBag.PolicyId = new SelectList(_db.Policies.OrderBy(p => p.Title), "Id", "Title", standard.PolicyId);
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", standard.OwnerUserId);
            return View(standard);
        }

        // POST: Standard/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id","PolicyId","Title","Description","Version","EffectiveDate","ReviewDate","OwnerUserId","Status")] Standard standard)
        {
            if (id != standard.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    standard.UpdatedAt = DateTime.UtcNow;
                    _db.Update(standard);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StandardExists(standard.Id))
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
            ViewBag.PolicyId = new SelectList(_db.Policies.OrderBy(p => p.Title), "Id", "Title", standard.PolicyId);
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", standard.OwnerUserId);
            return View(standard);
        }

        // GET: Standard/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _db.Standards
                .Include(s => s.Policy)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (standard == null)
            {
                return NotFound();
            }

            return View(standard);
        }

        // POST: Standard/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var standard = await _db.Standards.FindAsync(id);
            if (standard != null)
            {
                _db.Standards.Remove(standard);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StandardExists(int id)
        {
            return _db.Standards.Any(e => e.Id == id);
        }
    }
}
