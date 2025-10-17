using M_SAVA_Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services.Interfaces
{
    public interface ILoginService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken = default);
        Task<Guid> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken = default);
    }
}
