using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TKMelo.Library.DTOs.Openers;
using TKMelo.Library.Interfaces;
using TKMelo.Library.Options;
using TKMelo.Library.Prompts;

namespace TKMelo.Library.Services
{
    public class OpenersService : IOpenersService
    {
        private readonly HttpClient _http;
        private readonly OpenAIOptions _opts;

        public OpenersService(HttpClient http, IOptions<OpenAIOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
        }

        public async Task<OpenersResponse> GenerateAsync(OpenersRequest req, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_opts.ApiKey))
                throw new InvalidOperationException("Falta OpenAI:ApiKey en la configuración.");

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _opts.ApiKey);

            var payload = new
            {
                model = _opts.Model,
                input = new object[]
                {
                    new { role = "system", content = PromptLibrary.System },
                    new { role = "user",   content = PromptLibrary.BuildUser(req.Tone, req.Context, req.Language, req.Count) }
                },
                text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = "openers_schema",
                        schema = new
                        {
                            type = "object",
                            required = new[] { "openers" },
                            properties = new
                            {
                                openers = new
                                {
                                    type = "array",
                                    minItems = req.Count,
                                    maxItems = req.Count,
                                    items = new { type = "string" }
                                }
                            },
                            additionalProperties = false
                        },
                        strict = true
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var resp = await _http.PostAsync("https://api.openai.com/v1/responses", content, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"OpenAI error: {json}");

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement.GetProperty("output").EnumerateArray().First()
                         .GetProperty("content").EnumerateArray().First()
                         .GetProperty("text").GetString();

            var result = JsonSerializer.Deserialize<OpenersResponse>(text!)
                         ?? new OpenersResponse(Array.Empty<string>());

            return result;
        }
    }
}