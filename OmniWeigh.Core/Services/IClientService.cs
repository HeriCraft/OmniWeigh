using System.Collections.Generic;
using System.Threading.Tasks;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClientDto>> GetAllAsync();
        Task<ClientDto> AddAsync(ClientDto client);
    }
}
