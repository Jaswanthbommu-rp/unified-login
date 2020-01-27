import { EventEmitter } from '../../../stencil.core';
import { TabInterface } from '../../../utils/interfaces';
export declare class RaulTabs {
    xsDevice: boolean;
    /**
     * An array of objects representing the navigation.
     */
    tabs: Array<TabInterface>;
    /**
     * The name key of the active tab.
     */
    activeTab: string;
    /**
     * if `true`, tabs will render as a select on mobile devices.
     */
    selectOnMobile: boolean;
    /**
     * Emitted when a tab is clicked.
     */
    raulTabChange: EventEmitter;
    connectedCallback(): void;
    handleResize(): void;
    private selectOptions;
    private handleClick;
    private handleRaulChange;
    render(): any;
}
