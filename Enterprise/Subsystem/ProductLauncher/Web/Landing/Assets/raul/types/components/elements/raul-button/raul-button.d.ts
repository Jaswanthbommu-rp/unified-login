export declare class ButtonComponent {
    btnTextEl: HTMLElement;
    focusRingEl: HTMLDivElement;
    iconOnly: boolean;
    /**
     * Determines the primary appearance of the button based on its purpose.
     */
    variant: "primary" | "secondary" | "reverse" | "danger" | "text" | "control";
    /**
     * Determines the primary appearance of the button based on its purpose.
     */
    size: "default" | "small";
    /**
     * Controls the underlying markup based on the use case for the button.
     */
    type: "button" | "reset" | "submit";
    /**
     * Only valid for input types (submit, reset).
     */
    value: string;
    /**
     * Determines link behavior. Only valid for links.
     */
    href: string;
    /**
     * Controls whether this button is disabled.
     */
    disabled: boolean;
    /**
     * Adds `add` icon.
     */
    add: boolean;
    /**
     * Adds `delete` icon.
     */
    delete: boolean;
    /**
     * The button icon name.
     */
    icon: string;
    /**
     * The button icon kind.
     */
    iconKind: string;
    componentDidLoad(): void;
    render(): any;
}
