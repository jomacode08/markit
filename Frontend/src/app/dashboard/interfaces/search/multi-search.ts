export interface MultiSearchRequest {
    queries: Query[];
}

export interface Query {
    indexUid: string;
    q:        string;
    limit:    number;
    attributesToHighlight: string[];
    highlightPreTag: string;
    highlightPostTag: string;
}

export interface MultiSearchResponse {
    results: Result[];
}

export interface Result {
    indexUid:           string;
    hits:               Hit[];
    query:              string;
    processingTimeMs:   number;
    limit:              number;
    offset:             number;
    estimatedTotalHits: number;
}

export interface Hit {
    _formatted : any;
}