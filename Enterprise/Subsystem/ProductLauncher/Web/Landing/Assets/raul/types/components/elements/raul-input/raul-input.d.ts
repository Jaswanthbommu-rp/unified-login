export declare class RaulInput {
    el: HTMLInputElement;
    inputId: string;
    /**
     * Input's type.
     */
    type: 'text' | 'search';
    /**
     * Input's label text.
     */
    label: string;
    /**
     * Input's optional hint text.
     */
    hint: string;
    /**
     * Makes the input visually invalid and shows the feedback message.
     */
    error: string;
    componentDidLoad(): void;
    render(): any;
}
