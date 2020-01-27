import { EventEmitter } from '../../../stencil.core';
export declare class RaulModal {
    overlay: HTMLElement;
    modal: HTMLElement;
    /**
    * A `normal` modal will have a the header and body centered and no close button. A `media` modal will have the header content aligned to the left and a close button.
    */
    variant: 'normal' | 'media';
    /**
    * Determines wether the modal can be closed via clicking away or the `Escape` key
    */
    dismissable: boolean;
    /**
    * Event emitted when the modal should be closed (because user clicked the close button, clicked away, pressed `Escape` or chose an option)
    */
    modalClose: EventEmitter;
    componentDidLoad(): void;
    render(): any;
}
