export interface CollectionNode {
    key: string,
    label: string,
    data: string,
    parentKey?: string,
    children: CollectionNode[],
}