import { Editor } from '@tiptap/core';

export interface EditionOption {
    //* General properties
    /**
     * Text of the option
     */
    label: string,
    /**
     * Name of the node mark
     */
    name: string,
    /**
     * Icon of the option
     */
    icon: string,

    //* Optional properties
    level?: number,
    textAlign?: string,
    color?: string,
    children?: EditionOption[],

    /**
     * Callback to execute when option is clicked
     */
    command?(editor: Editor): void;
}