import { Block } from "./block";
import { Link } from "./link";

export interface Mark {
    id        : number;
    name      : string;
    creatorId : number;
    collectionId : number;
    createdDate ?: Date;
    links     ?: Link[];
    blocks    : Block[];
    emoji     ?: string;
    
    inputName ?: string;
    collectionName ?: string;
    requiresSync   ?: boolean;
}