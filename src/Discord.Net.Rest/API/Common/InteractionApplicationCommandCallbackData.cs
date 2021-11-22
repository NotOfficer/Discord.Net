using Newtonsoft.Json;

namespace Discord.API
{
    internal class InteractionApplicationCommandCallbackData
    {
        [JsonProperty("tts")]
        public Optional<bool> TTS { get; set; }

        [JsonProperty("content")]
        public Optional<string> Content { get; set; }

        [JsonProperty("embeds")]
        public Optional<Embed[]> Embeds { get; set; }

        [JsonProperty("allowed_mentions")]
        public Optional<AllowedMentions> AllowedMentions { get; set; }

        [JsonProperty("flags")]
        public Optional<int> Flags { get; set; }

        public InteractionApplicationCommandCallbackData() { }
        public InteractionApplicationCommandCallbackData(string text)
        {
            Content = text;
        }
    }
}
