namespace Bottlecap.Net.Bots.Alexa.Data
{
    public class DirectiveContent
    {
        public string type { get; set; }

        public string speech { get; set; }

        public DirectiveContent()
        {
            type = "VoicePlayer.Speak";
        }
    }
}