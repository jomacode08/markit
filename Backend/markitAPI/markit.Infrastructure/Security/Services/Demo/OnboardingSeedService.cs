using markit.Application.Contracts.Authentication.Demo;
using markit.Application.Contracts.Settings;
using markit.Domain.Entities;
using markit.Infrastructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Infrastructure.Security.Services.Demo
{
    /// <summary>
    /// Clones a pre-configured template user's collection directory into a guest user's
    /// workspace as part of demo session creation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The copy strategy uses a two-query, one-road-trip bulk insertion approach:
    /// <list type="number">
    ///   <item>The full template subtree is loaded in a single recursive CTE query.</item>
    ///   <item>
    ///     A batch of new Collection IDs is pre-allocated via
    ///     <c>SELECT nextval('collections_id_seq') FROM generate_series(1, N)</c>,
    ///     which allows <c>Path</c> and <c>ParentId</c> columns to be fully constructed
    ///     in memory before any insert is issued.
    ///   </item>
    ///   <item>All collections are inserted in a single parameterized raw SQL statement.</item>
    ///   <item>Notebooks and Blocks are inserted via EF <c>AddRange</c> + <c>SaveChangesAsync</c>.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public class OnboardingSeedService : IOnboardingSeedService
    {
        private readonly MarkitDbContext _context;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<OnboardingSeedService> _logger;

        private const string COLLECTIONS_SEQUENCE = "public.collections_id_seq";
        private const string CREATED_BY_SYSTEM = "System";
        private const string NO_TEMPLATE_CONFIGURED_MESSAGE = "OnboardingSeedService: No template user configured. Skipping seed.";
        private const string TEMPLATE_NOT_FOUND_MESSAGE = "OnboardingSeedService: Template user '{TemplateUserId}' has no directory. Skipping seed.";
        private const string SEED_COMPLETE_MESSAGE = "OnboardingSeedService: Seeded {CollectionCount} collections, {MarkCount} notebooks for guest '{GuestUserId}'.";

        public OnboardingSeedService(
            MarkitDbContext context,
            ISettingsService settingsService,
            ILogger<OnboardingSeedService> logger
        )
        {
            _context = context;
            _settingsService = settingsService;
            _logger = logger;
        }

        public async Task SeedAsync(string guestUserId)
        {
            string? templateUserId = await _settingsService.GetValueAsync(SystemConfigKeys.DEMO_USER_TEMPLATE_ID);
            if (string.IsNullOrWhiteSpace(templateUserId))
            {
                _logger.LogInformation(NO_TEMPLATE_CONFIGURED_MESSAGE);
                return;
            }

            // Phase 1: Load template collections and their notebooks + blocks
            List<Collection> templateDirectory = await GetTemplateDirectoryAsync(templateUserId);
            List<Collection> templateCollections = [.. templateDirectory.Where(c => !c.IsMain)];
            List<Notebook> templateNotebooks = [.. templateDirectory.SelectMany(c => c.Notebooks ?? [])];
            Collection? templateMainCollection = templateDirectory.FirstOrDefault(c => c.IsMain);
            Collection guestMainCollection = await GetGuestMainCollectionAsync(guestUserId);

            if (templateMainCollection is null || templateCollections.Count == 0)
            {
                _logger.LogWarning(TEMPLATE_NOT_FOUND_MESSAGE, templateUserId);
                return;
            }

            if (templateMainCollection.Description != guestMainCollection.Description)
            {
                await UpdateGuestMainCollectionDescriptionAsync(
                    guestMainCollection,
                    newDescription: templateMainCollection.Description
                );
            }

            // Phase 2: Pre-allocate collection IDs via nextval + generate_series
            int count = templateCollections.Count;
            List<int> newCollectionIds = await GetPreAllocateCollectionIdsAsync(count);
            Dictionary<int, int> idMap = ConstructIdMap(templateCollections, newCollectionIds);
            idMap.Add(templateMainCollection.Id, guestMainCollection.Id);

            // Phase 3: Construct and insert new collections sorted by depth (parents before children)
            List<Collection> sortedTemplate = [.. templateCollections.OrderBy(c => GetPathDepth(c.Path))];
            DateTime now = DateTime.UtcNow;

            List<Collection> newCollections = [.. sortedTemplate.Select(tc => new Collection
            {
                Id = idMap[tc.Id],
                Name = tc.Name,
                IsMain = false,
                ParentId = tc.ParentId.HasValue && idMap.TryGetValue(tc.ParentId.Value, out int mappedParentId) 
                    ? mappedParentId 
                    : guestMainCollection.Id,
                UserId = guestUserId,
                Path = RemapPathIds(tc.Path, idMap),
                PathNames = tc.PathNames,
                IsFavorite = tc.IsFavorite,
                Emoji = tc.Emoji,
                Enable = true,
                CreatedDate = now,
                CreatedBy = CREATED_BY_SYSTEM,
                Description = tc.Description,
            })];

            // Bulk insert collections via raw SQL (explicit IDs require bypassing EF identity)
            await BulkInsertCollectionsAsync(newCollections);

            // Phase 4: Build and insert notebooks + blocks via EF (auto-generated IDs)
            List<Notebook> newNotebooks = [.. templateNotebooks.Select(tm => new Notebook
            {
                CollectionId = idMap[tm.CollectionId],
                Name = tm.Name,
                IsFavorite = tm.IsFavorite,
                Emoji = tm.Emoji,
                NameLess = tm.NameLess,
                Enable = true,
                CreatedDate = now,
                CreatedBy = CREATED_BY_SYSTEM,
                Blocks = tm.Blocks?
                    .Where(b => b.Enable)
                    .Select(tb => new Block
                    {
                        Title = tb.Title,
                        Content = tb.Content,
                        Order = tb.Order,
                        Enable = true,
                        CreatedDate = now,
                        CreatedBy = CREATED_BY_SYSTEM
                    }).ToList()
            })];

            await BulkInsertNotebooksAsync(newNotebooks);
            _logger.LogInformation(SEED_COMPLETE_MESSAGE, newCollections.Count, newNotebooks.Count, guestUserId);
        }

        #region Helpers
        private static Dictionary<int, int> ConstructIdMap(List<Collection> templateCollections, List<int> newCollectionIds)
        {
            Dictionary<int, int> idMap = [];
            for (int i = 0; i < templateCollections.Count; i++)
            {
                idMap[templateCollections[i].Id] = newCollectionIds[i];
            }
            return idMap;
        }

        private static int GetPathDepth(string? path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            return path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private static string? RemapPathIds(string? path, Dictionary<int, int> idMap)
        {
            if (string.IsNullOrEmpty(path)) return path;

            string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var remapped = new string[segments.Length];
            for (int i = 0; i < segments.Length; i++)
            {
                remapped[i] = int.TryParse(segments[i], out int oldId) && idMap.TryGetValue(oldId, out int newId)
                    ? newId.ToString()
                    : segments[i];
            }
            return "/" + string.Join("/", remapped);
        }

        private async Task BulkInsertCollectionsAsync(List<Collection> collections)
        {
            StringBuilder sb = new();
            sb.Append(
                "INSERT INTO collections " +
                "(id, name, path, path_names, is_main, parent_id, user_id, is_favorite, emoji, created_date, created_by, updated_date, updated_by, enable, description) " +
                "VALUES "
            );

            List<NpgsqlParameter> dbParams = [];

            for (int i = 0; i < collections.Count; i++)
            {
                int b = i * 15;
                if (i > 0) sb.Append(',');
                sb.Append($"(@p{b},@p{b+1},@p{b+2},@p{b+3},@p{b+4},@p{b+5},@p{b+6},@p{b+7},@p{b+8},@p{b+9},@p{b+10},@p{b+11},@p{b+12},@p{b+13},@p{b+14})");

                Collection? col = collections[i];
                dbParams.Add(new NpgsqlParameter($"p{b}", NpgsqlDbType.Integer) { Value = col.Id });
                dbParams.Add(new NpgsqlParameter($"p{b+1}", NpgsqlDbType.Text) { Value = col.Name });
                dbParams.Add(new NpgsqlParameter($"p{b+2}", NpgsqlDbType.Text) { Value = (object?)col.Path ?? DBNull.Value });
                dbParams.Add(new NpgsqlParameter($"p{b+3}", NpgsqlDbType.Text) { Value = col.PathNames });
                dbParams.Add(new NpgsqlParameter($"p{b+4}", NpgsqlDbType.Boolean) { Value = col.IsMain });
                dbParams.Add(new NpgsqlParameter($"p{b+5}", NpgsqlDbType.Integer) { Value = (object?)col.ParentId ?? DBNull.Value });
                dbParams.Add(new NpgsqlParameter($"p{b+6}", NpgsqlDbType.Text) { Value = col.UserId });
                dbParams.Add(new NpgsqlParameter($"p{b+7}", NpgsqlDbType.Boolean) { Value = col.IsFavorite });
                dbParams.Add(new NpgsqlParameter($"p{b+8}", NpgsqlDbType.Text) { Value = (object?)col.Emoji ?? DBNull.Value });
                dbParams.Add(new NpgsqlParameter($"p{b+9}", NpgsqlDbType.TimestampTz) { Value = (object?)col.CreatedDate ?? DBNull.Value });
                dbParams.Add(new NpgsqlParameter($"p{b+10}", NpgsqlDbType.Text) { Value = (object?)col.CreatedBy ?? DBNull.Value });
                dbParams.Add(new NpgsqlParameter($"p{b+11}", NpgsqlDbType.TimestampTz) { Value = DBNull.Value });
                dbParams.Add(new NpgsqlParameter($"p{b+12}", NpgsqlDbType.Text) { Value = DBNull.Value });
                dbParams.Add(new NpgsqlParameter($"p{b+13}", NpgsqlDbType.Boolean) { Value = col.Enable });
                dbParams.Add(new NpgsqlParameter($"p{b+14}", NpgsqlDbType.Text) { Value = (object?)col.Description ?? DBNull.Value });
            }

            await _context.Database.ExecuteSqlRawAsync(sb.ToString(), [.. dbParams.Cast<object>()]);
        }

        private async Task BulkInsertNotebooksAsync(List<Notebook> notebooks)
        {
            _context.Notebooks.AddRange(notebooks);
            await _context.SaveChangesAsync();
        }

        private async Task<List<int>> GetPreAllocateCollectionIdsAsync(int count)
        {
            string sql = $"SELECT CAST(nextval('{COLLECTIONS_SEQUENCE}') AS integer) AS value FROM generate_series(1, {{0}})";
            return await _context.Database
                .SqlQueryRaw<int>(sql, count)
                .ToListAsync();
        }

        private async Task<Collection> GetGuestMainCollectionAsync(string guestUserId)
        {
            return await _context.Collections
                .FirstOrDefaultAsync(c => c.UserId == guestUserId && c.IsMain)
                ?? throw new InvalidOperationException($"Guest user {guestUserId} has no main collection. Account creation may have failed.");
        }

        private async Task<List<Collection>> GetTemplateDirectoryAsync(string templateUserId)
        {
            return await _context.Collections
                .Include(c => c.Notebooks!.Where(m => m.Enable))
                    .ThenInclude(m => m.Blocks!.Where(b => b.Enable))
                .Where(c => c.UserId == templateUserId && c.Enable)
                .AsSingleQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task UpdateGuestMainCollectionDescriptionAsync(Collection guestMainCollection, string? newDescription)
        {
            guestMainCollection.Description = newDescription;
            _context.Collections.Update(guestMainCollection);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
