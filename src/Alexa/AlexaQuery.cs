using Alexa.NET.Request.Type;
using Alexa.NET.Request;
using System;

namespace Bottlecap.Components.Bots.Alexa
{
    public class AlexaQuery : IBotQuery
    {
        private const string ALEXA_CANCEL_NAME = "AMAZON.CancelIntent";
        private const string ALEXA_STOP_NAME = "AMAZON.StopIntent";
        private const string ALEXA_YES_NAME = "AMAZON.YesIntent";
        private const string ALEXA_NO_NAME = "AMAZON.NoIntent";
        private const string ALEXA_NEXT_NAME = "AMAZON.NextIntent";

        private IntentRequest _request;

        public string Name {  get { return _request.Intent.Signature.FullName; } }

        public AlexaQuery(IntentRequest intent)
        {
            _request = intent;
        }

        public string GetTokenValue(string key)
        {
            Slot slot;
            if (_request.Intent.Slots.TryGetValue(key, out slot))
            {
                return slot.Value;
            }

            return null;
        }

        public bool HasToken(string key)
        {
            return _request.Intent.Slots.ContainsKey(key);
        }

        public bool IsNativeQuery(QueryType type)
        {
            switch(type)
            {
                case QueryType.Cancel:
                    return String.Equals(Name, ALEXA_CANCEL_NAME, StringComparison.OrdinalIgnoreCase);
                case QueryType.Next:
                    return String.Equals(Name, ALEXA_NEXT_NAME, StringComparison.OrdinalIgnoreCase);
                case QueryType.No:
                    return String.Equals(Name, ALEXA_NO_NAME, StringComparison.OrdinalIgnoreCase);
                case QueryType.Stop:
                    return String.Equals(Name, ALEXA_STOP_NAME, StringComparison.OrdinalIgnoreCase);
                case QueryType.Yes:
                    return String.Equals(Name, ALEXA_YES_NAME, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}