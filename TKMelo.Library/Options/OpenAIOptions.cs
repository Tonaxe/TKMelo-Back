using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKMelo.Library.Options
{
    public class OpenAIOptions
    {
        public const string SectionName = "OpenAI";
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gpt-4.1-mini";
    }
}
