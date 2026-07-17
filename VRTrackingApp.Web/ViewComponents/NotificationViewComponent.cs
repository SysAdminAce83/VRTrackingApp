using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.ViewComponents;

public class NotificationViewComponent : ViewComponent
{
    private readonly VRTrackingAppContext _db;
    public NotificationViewComponent(VRTrackingAppContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var uidStr = ViewContext.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var hasId = int.TryParse(uidStr, out var uid) && uid > 0;

        var unread = hasId
            ? await _db.Notifications.CountAsync(n => n.UserId == uid && !n.IsRead)
            : 0;
        var recent = hasId
            ? await _db.Notifications.Where(n => n.UserId == uid).OrderByDescending(n => n.CreatedAt).Take(6).ToListAsync()
            : new List<Notification>();

        ViewBag.Unread = unread;
        ViewBag.Recent = recent;
        return View();
    }
}
