export interface CollectionItem {
    id: string;
    name: string;
    collectionId : number;
    type: CollectionItemType;
    typeId : number;
    preview?: string;
    createdAt?: Date;
    updatedAt?: Date;
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