import { EventEmitter } from '../../../stencil.core';
export declare class RaulChip {
    chipTextEl: HTMLDivElement;
    title: string;
    /**
     * Makes the chip removable.
     */
    removable: boolean;
    /**
     * Emitted when the removable chip is clicked or delete/backspace keys are pressed.
     */
    raulChipRemove: EventEmitter;
    componentDidLoad(): void;
    private handleChipRemove;
    render(): any;
}
