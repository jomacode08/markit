import { Block } from "./block";
import { Link } from "./link";

export interface Mark {
    id        : number;
    name      : string;
    creatorId : number;
    collectionId : number;
    links     ?: Link[];
    blocks    : Block[];
    
    collectionName ?: string;
    requiresSync   ?: boolean;
}