import { EventEmitter } from '../../../stencil.core';
import { DropdownMenuItem } from '../../../utils/types';
export declare class RaulDropdownMenu {
    toggle: HTMLElement;
    dropdown: HTMLElement;
    open: boolean;
    top: boolean;
    right: boolean;
    openMutated: boolean;
    dividers: boolean;
    emphasizeFinal: boolean;
    items?: DropdownMenuItem[];
    color: 'active' | 'primary';
    disabled: boolean;
    optionSelected: EventEmitter;
    componentDidLoad(): void;
    handleOpenChange(newValue: any, oldValue: any): void;
    componentDidRender(): void;
    checkViewportCollision(): void;
    handleToggleClick: () => void;
    handleKeyDown: (e: any) => void;
    handleMenuItemClick: (payload: any) => void;
    handleMenuItemBlur: (e: any) => void;
    handleEscape: (e: any) => void;
    closeMenu(): Promise<void>;
    render(): any;
}
