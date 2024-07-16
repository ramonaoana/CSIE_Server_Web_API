using BaseLibrary.DTOs;

namespace BaseLibrary.Responses
{
    public record UserResponse(bool Flag, UserProfile user);
}
