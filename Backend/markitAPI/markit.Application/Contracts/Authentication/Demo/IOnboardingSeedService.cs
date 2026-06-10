namespace markit.Application.Contracts.Authentication.Demo
{
    /// <summary>
    /// Copies the template user's full collection directory (Collections, Notebooks and Blocks)
    /// into a newly created guest user's workspace during demo session onboarding.
    /// </summary>
    /// <remarks>
    /// The template user is resolved from the <c>Demo:UserTemplateId</c> system configuration key.
    /// If the key is absent or the template user has no sub-collections, the operation is a silent
    /// no-op and the session is created with only the default main collection.
    /// </remarks>
    public interface IOnboardingSeedService
    {
        /// <summary>
        /// Seeds the guest user's workspace by cloning the template directory tree.
        /// All seeded Collections, Notebooks and Blocks cascade-delete when the guest
        /// <see cref="markit.Application.Models.Authentication.AppUser.AppUser"/> is removed.
        /// </summary>
        /// <param name="guestUserId">The identity of the newly created guest account.</param>
        Task SeedAsync(string guestUserId);
    }
}
