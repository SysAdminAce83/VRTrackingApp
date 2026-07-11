using System.Collections.Generic;
using System.Threading.Tasks;
using VRTrackingApp.Domain.Models;
using VRTrackingApp.Infrastructure.Repositories;

namespace VRTrackingApp.Infrastructure.Interfaces
{
    public interface IAssetRepository : IBaseRepository<Asset>
    {
        Task<IReadOnlyList<Asset>> GetByScanIdAsync(int scanId);
        Task<Asset?> GetByIpAddressAsync(string ipAddress);
        Task<IReadOnlyList<Asset>> GetByHostNameAsync(string hostName);
    }
}