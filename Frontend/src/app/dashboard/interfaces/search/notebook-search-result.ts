/**
 * Represents a search result for a Notebook
 */
export interface NotebookSearchResult {
    /** Unique identifier for the notebook */
    id: number;
    /** Display name of the notebook */
    name: string;
    /** Blocks within this notebook that match the search query */
    blocks: BlockSearchResult[];
}

/**
 * Represents a block within a notebook that matches the search query
 */
export interface BlockSearchResult {
    /** Unique identifier for the block */
    id: number;
    /** Title of the matching block */
    title: string;
    /** Highlighted text showing the search match */
    snippet: string;
}