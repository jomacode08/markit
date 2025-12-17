using markit.Application.Common.Exceptions;
using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Common.Helpers
{
    public static class Extension
    {
        public static string GetName(this LoginProvider loginProvider)
        {
            return Enum.GetName(typeof(LoginProvider), loginProvider)
                ?? throw new InvalidOperationException();
        }

        public static string GetName(this LoginPurpose loginPurpose)
        {
            return Enum.GetName(typeof(LoginPurpose), loginPurpose)
                ?? throw new InvalidOperationException();
        }

        public static void ValidateCreator(this Collection collection, int creatorId)
        {
            if (collection.CreatorId != creatorId)
            {
                throw new ForbiddenResourceException(
                    resource: "Collection",
                    resourceId: collection.Id,
                    creatorId
                );
            }
        }

        public static void ValidateCreator(this Mark mark, int creatorId)
        {
            ArgumentNullException.ThrowIfNull(mark.Collection, nameof(mark.Collection));
            if (mark.Collection.CreatorId != creatorId)
            {
                throw new ForbiddenResourceException(
                    resource: "Mark",
                    resourceId: mark.Id,
                    creatorId
                );
            }
        }
    }
}
