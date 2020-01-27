import { EventEmitter } from '../../../stencil.core';
export declare class RaulAside {
    private asideEl;
    private dialogEl;
    private dialogTransitionDuration;
    private asideTrigger;
    el: HTMLRaulAsideElement;
    visible: boolean;
    expanded: boolean;
    focused: boolean;
    dialogWidth: number;
    secondaryDialogWidth: number;
    size: 'small' | 'medium' | 'large';
    /**
     * Emitted when the aside opens.
     */
    raulAsideOpen: EventEmitter;
    /**
     * Emitted when the aside closes.
     */
    raulAsideClose: EventEmitter;
    componentDidLoad(): void;
    handleRaulAsideOpen(e: any): void;
    handleRaulAsideClose(e: any): void;
    /**
     * Opens the aside.
     * @returns {Promise<void>}
     */
    open(): Promise<void>;
    /**
     * Closes the aside.
     * @returns {Promise<void>}
     */
    close(): Promise<void>;
    private dialogOffsetX;
    private focus;
    private blur;
    render(): any;
}
