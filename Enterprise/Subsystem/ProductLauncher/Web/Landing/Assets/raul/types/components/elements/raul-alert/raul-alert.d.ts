export declare class RaulAlert {
    /**
     * Determines the color of the left bar
     */
    variant: 'information' | 'success' | 'warning' | 'danger';
    /**
     * A header
     */
    heading: string;
    /**
     * Alert text
     */
    content?: string;
    /**
     * Action link text
     */
    ctaMessage?: string;
    /**
     * An action url
     */
    ctaUrl?: string;
    /**
     * Corners can be rounded or not
     */
    rounded?: boolean;
    componentWillLoad(): void;
    render(): any;
}
