using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TKMelo.Library.DTOs.Openers;

namespace TKMelo.Library.Interfaces
{
    public interface IOpenersService
    {
        Task<OpenersResponse> GenerateAsync(OpenersRequest request, CancellationToken ct = default);
    }
}
