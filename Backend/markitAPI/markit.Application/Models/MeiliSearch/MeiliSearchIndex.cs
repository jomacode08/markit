namespace markit.Application.Models.MeiliSearch
{
    public record MeiliSearchIndex(string Uid, List<MeiliSearchAttributeIndex> Attributes);

    public record MeiliSearchAttributeIndex(
        string Name,
        bool Displayed,
        bool Searchable = false,
        bool Filterable = false
    );
}
