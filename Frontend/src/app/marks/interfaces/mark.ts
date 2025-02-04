import { Block } from "./block";
import { Link } from "./link";

export type Cols = 12 | 6;

export interface Mark {
    id        : number;
    name      : string;
    creatorId : number;
    links     : Link[];
    blocks    : Block[];
}