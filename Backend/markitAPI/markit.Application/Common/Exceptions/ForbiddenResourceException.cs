namespace markit.Application.Common.Exceptions
{
    public class ForbiddenResourceException(
        string resource,
        int resourceId,
        string userId
    )
        : ApplicationException($"User: {userId} is forbidden from accessing resource: { resource } - { resourceId } .")
    {}
}
