using System.ComponentModel.DataAnnotations;

namespace TKMelo.Api.Controllers
{
    public class ReplyFromImageForm
    {
        [Required] public IFormFile Image { get; set; } = default!;
        [Required, MinLength(2)] public string Language { get; set; } = "es";
        [Required, MinLength(3)] public string Tone { get; set; } = "gracioso";
        [Range(1, 5)] public int Count { get; set; } = 1;
    }
}
