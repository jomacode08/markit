export interface CollectionItem {
    id: string;
    name: string;
    type: CollectionItemType;
    typeId : number;
    collectionId ?: number;
    preview?: string;
    updateDate?: Date;
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