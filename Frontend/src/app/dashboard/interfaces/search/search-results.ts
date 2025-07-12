import { CollectionDocument } from "./documents/collection-document";
import { MarkDocument } from "./documents/mark-document";

export interface SearchResults {
    collectionDocuments: CollectionDocument[],
    markDocuments : MarkDocument[],
}