import { EventEmitter } from '../../../stencil.core';
import { ToggleInterface } from '../../../utils/interfaces';
export declare class RaulToggles {
    xsDevice: boolean;
    /**
     * An array of objects representing the navigation.
     */
    toggles: Array<ToggleInterface>;
    /**
     * The name key of the active toggle.
     */
    activeToggle: string;
    /**
     * If `true`, toggles will render as a select on mobile devices.
     */
    selectOnMobile: boolean;
    /**
     * If `true`, the width of the parent will be distributed equally to the toggles.
     */
    fullWidth: boolean;
    /**
     * Emitted when a toggle is clicked.
     */
    raulToggleChange: EventEmitter;
    connectedCallback(): void;
    handleResize(): void;
    private selectOptions;
    private handleClick;
    private handleRaulChange;
    render(): any;
}
