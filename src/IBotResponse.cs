namespace Bottlecap.Components.Bots
{
    public interface IBotResponse
    {
        string Speak { get; set; }

        bool IsSSML { get; set; }

        object Content { get; set; }

        UserResponse ExpectedUserResponse { get; set; }

        string Title { get; set; }

        PermissionType PermissionRequests { get; set; }
    }
}