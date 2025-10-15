using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKMelo.Library.Prompts
{
    public static class PromptLibrary
    {
        public static string System =>
            "Eres 'TKMelo', un coach joven (18-25). Natural e ingenioso un poco travieso. " +
            "Nada de cliches ni frases sosas. Frases cortas (máx. 20 palabras). " +
            "Adapta el tono (gracioso/directo/romántico). Que sea un poco picante un jugon. " +
            "Utiliza un lenguaje fluido y natural, es decir que sea mas humano que no sea tan perfecta la escritura.";

        public static string BuildUser(string tone, string? context, string language, int count) =>
            $"Idioma: {language}. Tono: {tone}. Dame exactamente {count} frases breves para iniciar conversación, " +
            "con vibe joven y natural." +
            (string.IsNullOrWhiteSpace(context) ? "" : $" Contexto: {context}");
    }
}
