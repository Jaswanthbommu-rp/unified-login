export declare class RaulTextarea {
    el: HTMLRaulTextareaElement;
    inputId: string;
    /**
     * Textarea's label text.
     */
    label: string;
    /**
     * Textarea's optional hint text.
     */
    hint: string;
    /**
     * Makes the textarea visually invalid and shows the error message.
     */
    error: string;
    componentDidLoad(): void;
    render(): any;
}
