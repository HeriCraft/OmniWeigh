using System.Collections.Generic;
using System.Threading.Tasks;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto> AddAsync(ProductDto dto, string? imageSourcePath = null);
    }
}
