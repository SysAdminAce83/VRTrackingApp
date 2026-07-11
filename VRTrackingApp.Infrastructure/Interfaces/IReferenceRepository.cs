using System.Collections.Generic;
using System.Threading.Tasks;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Infrastructure.Repositories;

namespace VRTrackingApp.Infrastructure.Interfaces
{
    public interface IReferenceRepository : IBaseRepository<Reference>
    {
        Task<IReadOnlyList<Reference>> GetByVulnerabilityIdAsync(int vulnerabilityId);
    }
}