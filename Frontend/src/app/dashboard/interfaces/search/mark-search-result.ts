/**
 * Represents a search result for a Mark
 */
export interface MarkSearchResult {
    /** Unique identifier for the mark */
    id: number;
    /** Display name of the mark */
    name: string;
    /** Blocks within this mark that match the search query */
    blocks: BlockSearchResult[];
}

/**
 * Represents a block within a mark that matches the search query
 */
export interface BlockSearchResult {
    /** Unique identifier for the block */
    id: number;
    /** Title of the matching block */
    title: string;
    /** Highlighted text showing the search match */
    snippet: string;
}