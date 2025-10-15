using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKMelo.Library.DTOs.Openers
{
    public record OpenersRequest
    {
        public string Tone { get; init; } = "gracioso";
        public string? Context { get; init; }
        public string Language { get; init; } = "es";
        public int Count { get; init; } = 3;
    }
}
