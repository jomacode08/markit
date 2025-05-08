import { Cols } from "./mark";

export interface Block {
    id      : number,
    title   : string,
    cols    : Cols,
    color   : BlockColors,
    content ?: string,
}

export enum BlockColors {
    neutral = "neutral",
    purple = "purple",
    red = "red",
    transparent = "transparent"
}
