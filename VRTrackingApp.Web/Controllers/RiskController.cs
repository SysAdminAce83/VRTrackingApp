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
    public class RiskController : Controller
    {
        private readonly VRTrackingAppContext _db;

        public RiskController(VRTrackingAppContext db)
        {
            _db = db;
        }

        // GET: Risk
        public async Task<IActionResult> Index(string? riskName, string? status)
        {
            ViewData["Title"] = "Risks";
            ViewBag.RiskName = riskName;
            ViewBag.Status = status;

            IQueryable<Risk> query = _db.Risks
                .Include(r => r.Owner);

            if (!string.IsNullOrEmpty(riskName))
            {
                query = query.Where(r => r.RiskName.Contains(riskName));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var risks = await query.AsNoTracking().ToListAsync();
            return View(risks);
        }

        // GET: Risk/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var risk = await _db.Risks
                .Include(r => r.Owner)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (risk == null)
            {
                return NotFound();
            }

            return View(risk);
        }

        // GET: Risk/Create
        public IActionResult Create()
        {
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName");
            return View();
        }

        // POST: Risk/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id","RiskName","Description","BusinessImpact","Likelihood","RiskScore","OwnerUserId","ReviewDate","Status","Notes")] Risk risk)
        {
            if (ModelState.IsValid)
            {
                risk.CreatedAt = DateTime.UtcNow;
                risk.UpdatedAt = DateTime.UtcNow;
                _db.Add(risk);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", risk.OwnerUserId);
            return View(risk);
        }

        // GET: Risk/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var risk = await _db.Risks.FindAsync(id);
            if (risk == null)
            {
                return NotFound();
            }
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", risk.OwnerUserId);
            return View(risk);
        }

        // POST: Risk/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id","RiskName","Description","BusinessImpact","Likelihood","RiskScore","OwnerUserId","ReviewDate","Status","Notes")] Risk risk)
        {
            if (id != risk.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    risk.UpdatedAt = DateTime.UtcNow;
                    _db.Update(risk);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RiskExists(risk.Id))
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
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", risk.OwnerUserId);
            return View(risk);
        }

        // GET: Risk/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var risk = await _db.Risks
                .Include(r => r.Owner)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (risk == null)
            {
                return NotFound();
            }

            return View(risk);
        }

        // POST: Risk/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var risk = await _db.Risks.FindAsync(id);
            if (risk != null)
            {
                _db.Risks.Remove(risk);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RiskExists(int id)
        {
            return _db.Risks.Any(e => e.Id == id);
        }
    }
}
