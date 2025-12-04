import { CollectionDocument } from "./documents/collection-document";
import { MarkDocument } from "./documents/mark-document";

export interface DocumentSearch {
    collections: CollectionDocument[],
    marks : MarkDocument[],
}