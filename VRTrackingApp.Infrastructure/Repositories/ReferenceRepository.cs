using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Infrastructure.Data;
using VRTrackingApp.Infrastructure.Interfaces;

namespace VRTrackingApp.Infrastructure.Repositories
{
    public class ReferenceRepository : BaseRepository<Reference>, IReferenceRepository
    {
        public ReferenceRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Reference>> GetByVulnerabilityIdAsync(int vulnerabilityId)
        {
            return await _dbContext.References
                .Where(r => r.VulnerabilityId == vulnerabilityId)
                .ToListAsync();
        }
    }
}