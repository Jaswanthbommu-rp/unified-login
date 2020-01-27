import { EventEmitter } from '../../../stencil.core';
export declare class RaulDropdownMenu {
    /**
    * If you provide an url, the raul-action-menu-item will render an `a` tag
    */
    url: string;
    /**
    * If true, the option will be disabled
    */
    disabled: boolean;
    /**
    * The raul-action-menu-item will pass the click event and the payload in the callback
    */
    clickCallback: Function;
    /**
    * The raul-action-menu-item will pass the blur event and the payload in the callback
    */
    blurCallback: Function;
    /**
    * Event emitted when an option is selected.
    */
    optionSelected: EventEmitter;
    /**
    * Any sort of data that should be passed in the optionSelected event.detail and the callback functions
    */
    payload: unknown;
    render(): any;
}
