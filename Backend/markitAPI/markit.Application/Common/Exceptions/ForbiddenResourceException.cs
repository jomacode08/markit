namespace markit.Application.Common.Exceptions
{
    public class ForbiddenResourceException(
        string resource,
        int resourceId,
        int creatorId
    )
        : ApplicationException($"Creator {creatorId} is forbidden from accessing resource: { resource } - { resourceId } .")
    {}
}
