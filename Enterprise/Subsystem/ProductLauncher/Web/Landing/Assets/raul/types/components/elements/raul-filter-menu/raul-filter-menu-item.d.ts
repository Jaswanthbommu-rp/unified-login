import { EventEmitter } from '../../../stencil.core';
export declare class RaulFilterMenuItem {
    /**
    * An icon to be rendered before the item
    */
    icon: string;
    /**
    * If true, the option will be disabled
    */
    disabled: boolean;
    /**
    * The raul-filter-menu-item will pass the click event and the payload in the callback
    */
    onClickCallback: Function;
    /**
    * The raul-filter-menu-item will pass the blur event and the payload in the callback
    */
    onBlurCallback: Function;
    /**
    * Any sort of data that should be passed in the optionSelected event.detail and the callback functions
    */
    payload: unknown;
    /**
  * Event emitted when an option is selected.
  */
    optionSelected: EventEmitter;
    render(): any;
}
