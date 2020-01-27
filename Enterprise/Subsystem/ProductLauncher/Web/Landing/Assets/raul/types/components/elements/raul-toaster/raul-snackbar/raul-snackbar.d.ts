export declare class RaulSnackbar {
    timeoutTimer: number;
    refreshTimer: number;
    timeoutLeft: number;
    timeoutStartedAt: number;
    el: HTMLElement;
    /**
  * Determines the color of the left bar
  */
    variant: 'information' | 'success' | 'warning' | 'danger';
    /**
  * Determines wether the snackbar has a close button. Defaults to `false`
  */
    dismissable: boolean;
    /**
  * The first line of text
  */
    heading: string;
    /**
  * The second line of text
  */
    content?: string;
    /**
  * The call-to-action text
  */
    ctaMessage?: string;
    /**
  * A call-to-action url
  */
    ctaUrl?: string;
    /**
  * A callback function hat will be called on CTA click if an url was not provided
  */
    ctaCallback?: Function;
    /**
  * The number of ms after which the snackbar self-destructs
  */
    timeout: number;
    createdAt: Date;
    refreshKey: number;
    componentWillLoad(): void;
    disconnectedCallback(): void;
    handleMouseenter(): void;
    handleMouseleave(): void;
    dismiss(): void;
    createdAtTimeAgo(): any;
    render(): any;
}
