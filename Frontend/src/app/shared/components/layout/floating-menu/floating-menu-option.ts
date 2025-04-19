export interface FloatingMenuOption {
    /**
     * Text of the option
     */
    label: string,
    /**
     * Icon of the option
     */
    icon: string,
    /**
     * Children options
     */
    children?: FloatingMenuOption[],
    /**
     * Color to display the icon
     */
    color? : string,
    /**
     * Determinate if the option is disabled
     */
    isDisabled?(): boolean;
    /**
     * Callback to execute when option is clicked
     */
    command?(): void;
    /**
     * Determinate if the option is active
     */
    isActive?(): boolean;
}