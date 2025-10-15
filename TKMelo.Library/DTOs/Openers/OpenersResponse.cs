using System.Text.Json.Serialization;

namespace TKMelo.Library.DTOs.Openers
{
    public record OpenersResponse([property: JsonPropertyName("openers")] string[] Openers);
}
