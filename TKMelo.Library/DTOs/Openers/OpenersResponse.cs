using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TKMelo.Library.DTOs.Openers
{
    public record OpenersResponse([property: JsonPropertyName("openers")] string[] Openers);
}
