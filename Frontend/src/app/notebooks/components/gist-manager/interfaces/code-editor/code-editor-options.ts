import { Setup, Theme } from "@acrodata/code-editor";

export interface CodeEditorOptions {
    disabled : boolean,
    readonly : boolean,
    theme : Theme,
    setup: Setup,
    placeHolder : string,
    language : string,
}