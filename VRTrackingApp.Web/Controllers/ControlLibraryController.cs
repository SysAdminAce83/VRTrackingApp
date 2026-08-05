using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers
{
    [Authorize(Roles = "Admin,Analyst")]
    public class ControlLibraryController : Controller
    {
        private readonly VRTrackingAppContext _db;

        public ControlLibraryController(VRTrackingAppContext db)
        {
            _db = db;
        }

        // GET: ControlLibrary
        public async Task<IActionResult> Index(string? controlId, string? domain)
        {
            ViewData["Title"] = "Control Library";
            ViewBag.ControlId = controlId;
            ViewBag.Domain = domain;

            var query = _db.ControlLibraries.AsNoTracking();

            if (!string.IsNullOrEmpty(controlId))
            {
                query = query.Where(c => c.ControlId.Contains(controlId));
            }

            if (!string.IsNullOrEmpty(domain))
            {
                query = query.Where(c => c.Domain.Contains(domain));
            }

            var controlLibrary = await query.ToListAsync();
            return View(controlLibrary);
        }

        // GET: ControlLibrary/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controlLibrary = await _db.ControlLibraries
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (controlLibrary == null)
            {
                return NotFound();
            }

            return View(controlLibrary);
        }

        // GET: ControlLibrary/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ControlLibrary/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id","ControlId","Domain","ControlName","ControlDescription","Objective","ControlOwner","Frequency","Evidence","TestSteps","RiskAddressed")] ControlLibrary controlLibrary)
        {
            if (ModelState.IsValid)
            {
                controlLibrary.CreatedAt = DateTime.UtcNow;
                controlLibrary.UpdatedAt = DateTime.UtcNow;
                _db.Add(controlLibrary);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(controlLibrary);
        }

        // GET: ControlLibrary/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controlLibrary = await _db.ControlLibraries.FindAsync(id);
            if (controlLibrary == null)
            {
                return NotFound();
            }
            return View(controlLibrary);
        }

        // POST: ControlLibrary/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id","ControlId","Domain","ControlName","ControlDescription","Objective","ControlOwner","Frequency","Evidence","TestSteps","RiskAddressed")] ControlLibrary controlLibrary)
        {
            if (id != controlLibrary.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    controlLibrary.UpdatedAt = DateTime.UtcNow;
                    _db.Update(controlLibrary);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ControlLibraryExists(controlLibrary.Id))
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
            return View(controlLibrary);
        }

        // GET: ControlLibrary/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controlLibrary = await _db.ControlLibraries
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (controlLibrary == null)
            {
                return NotFound();
            }

            return View(controlLibrary);
        }

        // POST: ControlLibrary/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var controlLibrary = await _db.ControlLibraries.FindAsync(id);
            if (controlLibrary != null)
            {
                _db.ControlLibraries.Remove(controlLibrary);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: ControlLibrary/ImportFromCsv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromCsv()
        {
            var basePath = @"D:\Project_WebApp\VRTrackingApp\CtrlLIB";
            var controlLibraryCsv = Path.Combine(basePath, "ControlLibrary.csv");

            if (!System.IO.File.Exists(controlLibraryCsv))
            {
                return NotFound("ControlLibrary.csv not found.");
            }

            var lines = System.IO.File.ReadAllLines(controlLibraryCsv);
            if (lines.Length == 0)
            {
                return BadRequest("ControlLibrary.csv is empty.");
            }

            // Skip header
            var dataLines = lines.Skip(1);

            var controlList = new List<ControlLibrary>();

            foreach (var line in dataLines)
            {
                var values = ParseCsvLine(line);
                if (values.Length < 9)
                {
                    continue; // skip invalid lines
                }

                var control = new ControlLibrary
                {
                    ControlId = values[0].Trim('"'),
                    Domain = values[1].Trim('"'),
                    ControlName = values[2].Trim('"'),
                    ControlDescription = values[3].Trim('"'),
                    ControlOwner = values[4].Trim('"'),
                    Frequency = values[5].Trim('"'),
                    Evidence = values[6].Trim('"'),
                    TestSteps = values[7].Trim('"'),
                    RiskAddressed = values[8].Trim('"'),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                controlList.Add(control);
            }

            var objectiveDict = new Dictionary<string, string>();
            var objectiveFiles = Directory.GetFiles(basePath, "*_ID-Control-Objective.csv");
            foreach (var file in objectiveFiles)
            {
                var objLines = System.IO.File.ReadAllLines(file);
                if (objLines.Length == 0) continue;
                var objDataLines = objLines.Skip(1);
                foreach (var objLine in objLines.Skip(1))
                {
                    var objValues = ParseCsvLine(objLine);
                    if (objValues.Length >= 3)
                    {
                        var id = objValues[0].Trim('"');
                        var objective = objValues[2].Trim('"');
                        if (!string.IsNullOrEmpty(id))
                        {
                            objectiveDict[id] = objective;
                        }
                    }
                }
            }

            foreach (var control in controlList)
            {
                if (objectiveDict.TryGetValue(control.ControlId, out var objective))
                {
                    control.Objective = objective;
                }
            }

            _db.ControlLibraries.AddRange(controlList);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ControlLibraryExists(int id)
        {
            return _db.ControlLibraries.Any(e => e.Id == id);
        }

        private string[] ParseCsvLine(string line)
        {
            var list = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    list.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            list.Add(current.ToString());
            return list.ToArray();
        }
    }
}
