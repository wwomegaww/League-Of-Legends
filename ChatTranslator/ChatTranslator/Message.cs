using EloBuddy;

namespace ChatTranslator
{
    internal class Message
    {
        public string Translated;
        public string Original;
        public Obj_AI_Base Sender;

        public Message(string translated, Obj_AI_Base sender, string original)
        {
            Translated = translated;
            Sender = sender;
            Original = original;
        }
    }
}