using markit.Application.Models.MeiliSearch.Documents;
using System.Text.Json.Serialization;

namespace markit.Application.Models.MeiliSearch.Search
{
    /// <summary>
    /// Represents a document with a formatted version.
    /// This class is necessary to access MeiliSearch "_formatted" sub-object returned within each search.
    /// </summary>
    /// <typeparam name="T">The type of the document content, which must derive from <see cref="Document"/>.</typeparam>
    public class FormattedDocument<T> where T : Document
    {
        [JsonPropertyName("_formatted")]
        public required T Formatted { get; set; }
    }
}
