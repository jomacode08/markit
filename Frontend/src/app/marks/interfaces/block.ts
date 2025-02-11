import { Cols } from "./mark";

export interface Block {
    id      : number,
    cols    : Cols,
    color   : BackColors,
    content ?: string,
}

export enum BackColors {
    neutral = "neutral",
    purple = "purple",
    red = "red"
}
