export interface CollectionItem {
    id: string;
    name: string;
    type: CollectionItemType;
    typeId : number;
    collectionId : number;
    preview?: string;
    updateDate?: Date;
}

export interface CollectionItemFilter {
    collectionId : number;
    page: number;
    pageSize: number;
    collectionItemCategory : CollectionItemCategory
}

export enum CollectionItemType {
    Collection = 'collection',
    Mark = 'mark'
}

export enum CollectionItemCategory {
    All = 'All',
    Collections = 'Collections',
    Marks = 'Marks'
}

export enum CollectionItemAction {
    Add = 'Add',
    Rename = 'Rename',
    Delete = 'Delete'
}