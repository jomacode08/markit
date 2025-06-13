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
}

export enum CollectionItemType {
    Collection = 'collection',
    Mark = 'mark'
}

export enum CollectionItemAction {
    Add = 'Add',
    Rename = 'Rename',
    Delete = 'Delete'
}