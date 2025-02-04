export interface FloatingMenuOption {
    //* General properties
    /**
     * Text of the option
     */
    label: string,
    /**
     * Icon of the option
     */
    icon: string,

    children?: FloatingMenuOption[],
    color? : string,

    /**
     * Callback to execute when option is clicked
     */
    command?(): void;
    /**
     * Determinate if the option is active
     */
    isActive?(): boolean;
}