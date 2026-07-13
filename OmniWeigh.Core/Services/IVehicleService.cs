using System.Collections.Generic;
using System.Threading.Tasks;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetAllAsync();
        Task<VehicleDto> AddAsync(VehicleDto dto, string? imageSourcePath = null);
    }
}
