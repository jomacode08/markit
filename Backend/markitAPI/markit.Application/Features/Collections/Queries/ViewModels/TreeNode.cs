namespace markit.Application.Features.Collections.Queries.ViewModels
{
    public class TreeNode
    {
        public required string Key { get; set; }
        public required string Label { get; set; }
        public required string Data { get; set; }
        public string? ParentKey { get; set; }
        public IEnumerable<TreeNode> Children { get; set; } = [];
    }
}
