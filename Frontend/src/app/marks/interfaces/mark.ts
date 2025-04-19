import { Block } from "./block";
import { Link } from "./link";

export type Cols = 12 | 6;

export interface Mark {
    id        : number;
    name      : string;
    creatorId : number;
    collectionId : number
    links     ?: Link[];
    blocks    : Block[];
}