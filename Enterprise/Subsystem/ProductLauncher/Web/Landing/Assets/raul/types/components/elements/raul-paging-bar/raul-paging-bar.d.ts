import { EventEmitter } from '../../../stencil.core';
export declare class RaulPagingBar {
    currentPage: number;
    totalPages: number;
    startRow: number;
    endRow: number;
    entities: any;
    totalRows: number;
    rowsPerPage: number;
    pagingChange: EventEmitter;
    componentDidLoad(): void;
    decrement(): void;
    entitesChanged(event: any): void;
    increment(): void;
    pageValueChange(event: any): void;
    updateBar(event: string): void;
    validateRowsPerPage(): any;
    render(): any;
}
