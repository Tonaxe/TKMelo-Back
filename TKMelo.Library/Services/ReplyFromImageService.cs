using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TKMelo.Library.DTOs.Openers;
using TKMelo.Library.Interfaces;
using TKMelo.Library.Options;
using TKMelo.Library.Prompts;

namespace TKMelo.Library.Services
{
    public class ReplyFromImageService : IReplyFromImageService
    {
        private readonly HttpClient _http;
        private readonly OpenAIOptions _opts;

        public ReplyFromImageService(HttpClient http, IOptions<OpenAIOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
        }

        public async Task<ReplyFromImageResponse> GenerateAsync(ReplyFromImageRequest req, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(req.ImageBase64))
                throw new InvalidOperationException("Falta la imagen en base64.");

            var imageUrl = req.ImageBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                ? req.ImageBase64
                : $"data:image/png;base64,{req.ImageBase64}";

            if (string.IsNullOrWhiteSpace(_opts.ApiKey))
                throw new InvalidOperationException("Falta OpenAI:ApiKey en la configuración.");

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _opts.ApiKey);

            var payload = new
            {
                model = _opts.Model,
                input = new object[]
                {
                    new { role = "system", content = ReplyFromImagePrompts.System },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "input_text", text = ReplyFromImagePrompts.User(req.Language, req.Tone, req.Count) },
                            new { type = "input_image", image_url = imageUrl }
                        }
                    }
                },
                text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = "reply_from_image_schema",
                        schema = new
                        {
                            type = "object",
                            required = new[] { "transcript", "bestReply", "alternatives" },
                            properties = new
                            {
                                transcript = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        required = new[] { "speaker", "text" },
                                        properties = new
                                        {
                                            speaker = new { type = "string", @enum = new[] { "yo", "ella" } },
                                            text = new { type = "string" }
                                        },
                                        additionalProperties = false
                                    }
                                },
                                bestReply = new { type = "string" },
                                alternatives = new
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

            using var outer = JsonDocument.Parse(json);
            var textJson = outer.RootElement
                .GetProperty("output").EnumerateArray().First()
                .GetProperty("content").EnumerateArray().First()
                .GetProperty("text").GetString();

            if (string.IsNullOrWhiteSpace(textJson))
                throw new InvalidOperationException("No se pudo leer la salida del modelo.");

            using var parsed = JsonDocument.Parse(textJson);
            var transcript = parsed.RootElement.GetProperty("transcript")
                .EnumerateArray()
                .Select(t => new TranscriptTurn(
                    t.GetProperty("speaker").GetString() ?? "yo",
                    t.GetProperty("text").GetString() ?? string.Empty
                ))
                .ToArray();

            var best = parsed.RootElement.GetProperty("bestReply").GetString() ?? string.Empty;
            var alts = parsed.RootElement.GetProperty("alternatives")
                .EnumerateArray()
                .Select(a => a.GetString() ?? string.Empty)
                .ToArray();

            return new ReplyFromImageResponse(transcript, best, alts);
        }
    }
}