import { EventEmitter } from '../../../../stencil.core';
export declare class RaulGridSorter {
    direction: 'ascending' | 'descending' | null;
    field: string;
    private handleSort;
    raulSort: EventEmitter;
    render(): any;
}
