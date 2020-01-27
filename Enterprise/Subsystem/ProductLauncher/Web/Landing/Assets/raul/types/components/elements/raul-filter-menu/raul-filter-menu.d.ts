import { EventEmitter } from '../../../stencil.core';
import { DropdownMenuItem } from '../../../utils/types';
export declare class RaulDropdownMenu {
    dropdownMenu: HTMLRaulDropdownMenuElement;
    /**
    * As an alternative to the <raul-filter-menu-item>, you can programatically provide an array of items to be shown in the dropdown. {title: `string`, icon: `string`, payload: `any`}.
    * Payload will be the detail of the `optionSelected` event emitted when clicking an action
    * */
    items: DropdownMenuItem[];
    /**
    * Icon to be used in the menu toggle. Defaults to `list-bullets-3`
    */
    icon: string;
    /**
    * Method to programatically close the menu
    */
    closeMenu(): Promise<void>;
    /**
    * Event emitted when an option is selected.
    */
    optionSelected: EventEmitter;
    render(): any;
}
