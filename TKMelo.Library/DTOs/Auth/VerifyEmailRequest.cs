using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKMelo.Library.DTOs.Auth
{
    public class VerifyEmailRequest
    {
        public string Token { get; init; } = default!;
    }
}
