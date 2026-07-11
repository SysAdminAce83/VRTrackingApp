using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Infrastructure.Data;
using VRTrackingApp.Infrastructure.Interfaces;

namespace VRTrackingApp.Infrastructure.Repositories
{
    public class AssetRepository : BaseRepository<Asset>, IAssetRepository
    {
        public AssetRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Asset>> GetByScanIdAsync(int scanId)
        {
            return await _dbContext.Assets
                .Where(a => a.ScanId == scanId)
                .ToListAsync();
        }

        public async Task<Asset?> GetByIpAddressAsync(string ipAddress)
        {
            return await _dbContext.Assets
                .FirstOrDefaultAsync(a => a.IPAddress == ipAddress);
        }

        public async Task<IReadOnlyList<Asset>> GetByHostNameAsync(string hostName)
        {
            return await _dbContext.Assets
                .Where(a => a.HostName == hostName)
                .ToListAsync();
        }
    }
}