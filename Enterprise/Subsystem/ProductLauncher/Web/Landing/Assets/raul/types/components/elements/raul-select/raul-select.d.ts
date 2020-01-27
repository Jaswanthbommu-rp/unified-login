import { EventEmitter } from '../../../stencil.core';
import { PopperPlacements } from '../../../utils/types';
export declare class RaulSelect {
    private selectEl;
    private searchInputEl;
    private toggleButtonEl;
    private selectMenuEl;
    private processedOptions;
    private popper;
    el: HTMLRaulSelectElement;
    focusedOptionId: string;
    opened: boolean;
    expanded: boolean;
    searchQuery: string;
    /**
     * If `true`, the user cannot interact with the select.
     */
    disabled: boolean;
    /**
     * If `true`, allows multiple selections.
     */
    multiple: boolean;
    /**
     * The name of the control, which is submitted with the form data.
     */
    name: string;
    /**
     * The text label.
     */
    label: string;
    /**
     * Optional hint text.
     */
    hint: string;
    /**
     * Makes the select visually invalid and shows the feedback message.
     */
    error: string;
    /**
     * Sets the max height of the select options list, e.g. `500px`, `calc(100vh - 50px)`.
     */
    optionsMaxHeight: string;
    /**
     * The text to display when the select value is empty.
     */
    placeholder: string;
    /**
     * If `true`, adds a search field at the top of the select menu.
     */
    searchable: boolean;
    /**
     * If `true`, removes select toggle border.
     */
    borderless: boolean;
    /**
     * The value of the select.
     */
    value: string | string[];
    /**
     * Options and groups of the select.
     */
    options: Object[];
    /**
     * Options menu's placement.
     */
    menuPlacement: PopperPlacements;
    /**
     * If `true` the menu position will be 'fixed'.
     */
    menuPositionFixed: boolean;
    optionsChanged(): void;
    valueChanged(): void;
    /**
     * Emitted when the select menu opens.
     */
    raulSelectOpen: EventEmitter;
    /**
     * Emitted when the select menu closes.
     */
    raulSelectClose: EventEmitter;
    /**
     * Emitted when the select value changes.
     */
    raulChange: EventEmitter;
    componentWillLoad(): void;
    handleKeyDown(e: any): void;
    handleClickAndFocusOutside(e: any): void;
    handleDebouncedResize(): void;
    /**
     * Open the select menu.
     */
    open(): Promise<void>;
    /**
     * Close the select menu.
     */
    close(): Promise<void>;
    /**
     * Toggle the select menu.
     */
    toggle(): Promise<void>;
    private hasSlot;
    private matchSearchQuery;
    private visibleOptionsAndGroups;
    private optionsWithoutGroups;
    private focusableOptionsAndGroups;
    private selectedOptions;
    private selectedOptionsText;
    private processOptions;
    private validateValue;
    private focusOption;
    private handleOptionChange;
    private handleOptionsGroupChange;
    private handleSearch;
    private createPopper;
    private destroyPopper;
    private clearSearch;
    render(): any[];
}
