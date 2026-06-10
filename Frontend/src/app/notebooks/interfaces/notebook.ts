import { Block } from "./block";

export interface Notebook {
    id: number;
    name: string;
    userId: string;
    collectionId: number;
    createdDate?: Date;
    blocks: Block[];
    emoji?: string;
    inputName?: string;
    collectionName?: string;
    requiresSync?: boolean;
}