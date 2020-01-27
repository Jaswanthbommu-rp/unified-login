import { EventEmitter } from '../../../stencil.core';
import { DropdownMenuItem } from '../../../utils/types';
export declare class RaulActionMenu {
    dropdownMenu: HTMLRaulDropdownMenuElement;
    /**
     * As an alternative to the <raul-action-menu-item>, you can programatically provide an array of items to be shown in the dropdown. {title: `string`, url?: `string`, payload: `any`}.
     * Payload will be the detail of the `optionSelected` event emitted when clicking an action that doesn't have an url
     */
    items: DropdownMenuItem[];
    /**
     * If set to true, the last action will be separated with a divider
     */
    emphasizeFinal: boolean;
    /**
     * Disables actions
     */
    disabled: boolean;
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
