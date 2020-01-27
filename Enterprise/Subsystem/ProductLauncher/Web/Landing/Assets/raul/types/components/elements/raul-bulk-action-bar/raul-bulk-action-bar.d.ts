import { EventEmitter } from '../../../stencil.core';
export declare class RaulBulkActionBar {
    buttonWidths: any;
    bulkActionEl: HTMLDivElement;
    moreTray: HTMLDivElement;
    toggleOverflow: boolean;
    selectedCount?: number;
    totalRecords?: number;
    open: boolean;
    buttonsInTray: any;
    showTray: boolean;
    openChanged(): void;
    bulkActionsClose: EventEmitter;
    bulkActionsSelectAll: EventEmitter;
    handleResize(): void;
    getButtonWidths(): void;
    closeBar(): void;
    debounce(func: any, delay: any): () => void;
    toggleTray(): void;
    handelMoreButtons(element: any, buttonWidths: any, buttons: any): void;
    render(): any;
}
