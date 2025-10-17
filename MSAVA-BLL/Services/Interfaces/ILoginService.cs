using MSAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSAVA_BLL.Services.Interfaces
{
    public interface ILoginService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default);
        Task<Guid> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken = default);
    }
}
