using System;

namespace Bottlecap.Components.Bots
{
    public class BotResponse : IBotResponse
    {
        public string Speak { get; set; }
        public bool IsSSML { get; set; }
        public string Title { get; set; }
        public object Content { get; set; }
        public PermissionType PermissionRequests { get; set; }
        public UserResponse ExpectedUserResponse { get; set; }
    }
}
