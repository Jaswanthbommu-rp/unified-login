export declare class RaulBadge {
    /**
     * Determines the primary appearance of the badge based on its purpose.
     */
    variant: 'primary' | 'warning' | 'error' | 'success';
    /**
     * The icon to display to the left of the content.
     */
    icon: string;
    /**
     * The text or number to display in the badge.
     */
    content: string;
    render(): any;
}
