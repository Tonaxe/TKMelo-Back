namespace TKMelo.Library.DTOs.Openers
{
    public record ReplyFromImageRequest
    {
        public string ImageBase64 { get; init; } = "";
        public string Language { get; init; } = "es";
        public string Tone { get; init; } = "gracioso";
        public int Count { get; init; } = 1;
    }
}
