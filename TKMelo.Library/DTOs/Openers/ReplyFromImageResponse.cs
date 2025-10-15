namespace TKMelo.Library.DTOs.Openers
{
    public record TranscriptTurn(string Speaker, string Text);
    public record ReplyFromImageResponse(TranscriptTurn[] Transcript, string BestReply, string[] Alternatives);
}
