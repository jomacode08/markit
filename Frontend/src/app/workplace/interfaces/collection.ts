import { CollectionItem } from "./collection-item";

export interface Collection {
    id: number;
    name: string;
    isMain : boolean;
    creatorId ?: number;
    parentId  ?: number;
    emoji ?: string;
    collectionItems ?: CollectionItem[];
    path ?: CollectionPath[];
}

export interface CollectionPath {
    collectionId: number;
    name: string;
    emoji ?: string;
}