using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Infrastructure.Data;
using VRTrackingApp.Infrastructure.Interfaces;

namespace VRTrackingApp.Infrastructure.Repositories
{
    public class ScanRepository : BaseRepository<Scan>, IScanRepository
    {
        public ScanRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Scan?> GetByScanNameAsync(string scanName)
        {
            return await _dbContext.Scans
                .FirstOrDefaultAsync(s => s.ScanName == scanName);
        }

        public async Task<IReadOnlyList<Scan>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Scans
                .Where(s => s.ScanDate >= startDate && s.ScanDate <= endDate)
                .OrderByDescending(s => s.ScanDate)
                .ToListAsync();
        }
    }
}