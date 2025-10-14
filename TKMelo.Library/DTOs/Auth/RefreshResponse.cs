using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKMelo.Library.DTOs.Auth
{
    public class RefreshResponse
    {
        public string AccessToken { get; init; } = default!;
        public DateTimeOffset AccessTokenExpiresAt { get; init; }

        public string RefreshToken { get; init; } = default!;
        public DateTimeOffset RefreshTokenExpiresAt { get; init; }
    }
}
