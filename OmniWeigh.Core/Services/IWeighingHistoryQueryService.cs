using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public interface IWeighingHistoryQueryService
    {
        Task<PagedResult<WeighingHistoryItemDto>> GetHistoryAsync(WeighingHistoryFilterDto filter);
    }
}
