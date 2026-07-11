using System.Collections.Generic;
using System.Threading.Tasks;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Infrastructure.Repositories;

namespace VRTrackingApp.Infrastructure.Interfaces
{
    public interface IScanRepository : IBaseRepository<Scan>
    {
        Task<Scan?> GetByScanNameAsync(string scanName);
        Task<IReadOnlyList<Scan>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}