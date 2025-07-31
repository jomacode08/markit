export interface CollectionItem {
    id: string;
    name: string;
    collectionId : number;
    type: CollectionItemType;
    typeId : number;
    isFavorite : boolean;
    preview?: string;
    createdAt?: Date;
    updatedAt?: Date;
    //* Utility properties
    updating?: boolean;
    emoji?: string;
}

export enum CollectionItemType {
    Collection = 'collection',
    Mark = 'mark'
}

export enum CollectionItemAction {
    Add = 'New',
    Rename = 'Rename',
    Delete = 'Delete'
}