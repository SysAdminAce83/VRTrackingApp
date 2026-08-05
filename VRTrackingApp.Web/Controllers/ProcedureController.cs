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
    public class ProcedureController : Controller
    {
        private readonly VRTrackingAppContext _db;

        public ProcedureController(VRTrackingAppContext db)
        {
            _db = db;
        }

        // GET: Procedure
        public async Task<IActionResult> Index(string? title, string? status)
        {
            ViewData["Title"] = "Procedures";
            ViewBag.Title = title;
            ViewBag.Status = status;

            var query = _db.Procedures
                .Include(p => p.Standard)
                .ThenInclude(s => s.Policy)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(p => p.Title.Contains(title));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var procedures = await query.ToListAsync();
            return View(procedures);
        }

        // GET: Procedure/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _db.Procedures
                .Include(p => p.Standard)
                .ThenInclude(s => s.Policy)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (procedure == null)
            {
                return NotFound();
            }

            return View(procedure);
        }

        // GET: Procedure/Create
        public IActionResult Create()
        {
            ViewBag.StandardId = new SelectList(_db.Standards.Include(s => s.Policy).OrderBy(s => s.Title), "Id", "Title");
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName");
            return View();
        }

        // POST: Procedure/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id","StandardId","Title","Description","Version","EffectiveDate","ReviewDate","OwnerUserId","Status")] Procedure procedure)
        {
            if (ModelState.IsValid)
            {
                procedure.CreatedAt = DateTime.UtcNow;
                procedure.UpdatedAt = DateTime.UtcNow;
                _db.Add(procedure);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.StandardId = new SelectList(_db.Standards.Include(s => s.Policy).OrderBy(s => s.Title), "Id", "Title", procedure.StandardId);
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", procedure.OwnerUserId);
            return View(procedure);
        }

        // GET: Procedure/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _db.Procedures.FindAsync(id);
            if (procedure == null)
            {
                return NotFound();
            }
            ViewBag.StandardId = new SelectList(_db.Standards.Include(s => s.Policy).OrderBy(s => s.Title), "Id", "Title", procedure.StandardId);
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", procedure.OwnerUserId);
            return View(procedure);
        }

        // POST: Procedure/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id","StandardId","Title","Description","Version","EffectiveDate","ReviewDate","OwnerUserId","Status")] Procedure procedure)
        {
            if (id != procedure.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    procedure.UpdatedAt = DateTime.UtcNow;
                    _db.Update(procedure);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcedureExists(procedure.Id))
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
            ViewBag.StandardId = new SelectList(_db.Standards.Include(s => s.Policy).OrderBy(s => s.Title), "Id", "Title", procedure.StandardId);
            ViewBag.OwnerUserId = new SelectList(_db.UserAccounts.OrderBy(u => u.UserName), "Id", "UserName", procedure.OwnerUserId);
            return View(procedure);
        }

        // GET: Procedure/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var procedure = await _db.Procedures
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (procedure == null)
            {
                return NotFound();
            }

            return View(procedure);
        }

        // POST: Procedure/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var procedure = await _db.Procedures.FindAsync(id);
            if (procedure != null)
            {
                _db.Procedures.Remove(procedure);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProcedureExists(int id)
        {
            return _db.Procedures.Any(e => e.Id == id);
        }
    }
}
