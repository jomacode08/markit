using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using markit.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace markit.Infrastructure.Repositories.Marks
{
    public class BlockRepository : BaseRepository<Block>, IBlockRepository
    {
        public BlockRepository(MarkitDbContext mirefDbContext) : base(mirefDbContext)
        {
        }

        public async Task<IEnumerable<BlockSearchResult>> SearchAsync(string searchTerm, string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return [];

            const string HEADLINE_OPTIONS = "StartSel=<mark>, StopSel=</mark>, MaxWords=30, MinWords=15, MaxFragments=1";
            const string NO_CONTENT_RESULT = "No content founded";
            const string LANGUAGE_CONFIGURATION = "English";
            const string SEARCH_VECTOR_SHADOW_PROPERTY_NAME = "SearchVector";
            
            List<BlockSearchResult> results = await context.Blocks
                // Query processing
                .Include(b => b.Mark)
                .ThenInclude(m => m!.Collection)
                .Where(b => b.Mark!.Collection!.UserId == userId)
                .Where(b =>
                    EF.Property<NpgsqlTsVector>(b, SEARCH_VECTOR_SHADOW_PROPERTY_NAME)
                        .Matches(EF.Functions.WebSearchToTsQuery(LANGUAGE_CONFIGURATION, searchTerm))
                )
                // Ranking implementation
                .OrderByDescending(b => 
                    EF.Property<NpgsqlTsVector>(b, SEARCH_VECTOR_SHADOW_PROPERTY_NAME)
                        .Rank(EF.Functions.WebSearchToTsQuery(LANGUAGE_CONFIGURATION, searchTerm))
                )
                .Take(10)
                // Projection
                .Select(b =>  new BlockSearchResult
                {
                    Id = b.Id,
                    Title = b.Title,
                    // Content highlighting
                    Snippet = b.Content != null 
                        ? EF.Functions.WebSearchToTsQuery(LANGUAGE_CONFIGURATION, searchTerm)
                            .GetResultHeadline(LANGUAGE_CONFIGURATION, b.Content, HEADLINE_OPTIONS)
                        : NO_CONTENT_RESULT,
                    MarkId = b.MarkId,
                    MarkName = b.Mark!.Name
                })
                .ToListAsync(cancellationToken);

            // Sanitize snippet output
            foreach (BlockSearchResult result in results)
            {
                result.Snippet = Utilities.SanitizeHtml(result.Snippet);
            }

            return results;
        }
    }
}
