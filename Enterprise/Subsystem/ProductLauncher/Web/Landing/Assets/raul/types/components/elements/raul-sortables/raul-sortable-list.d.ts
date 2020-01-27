import { EventEmitter } from '../../../stencil.core';
export declare class RaulSortableList {
    el: HTMLElement;
    /**
     * If two or more lists have the same `group` property, the user will be able to drag & drop items between them
     */
    group: string;
    /**
     * Label of a list group
     */
    listGroupLabel: string;
    /**
     * The event will be emitted at the end of a drag&drop
     */
    itemDrag: EventEmitter;
    emitItemDragEvent(e: any): void;
    componentDidLoad(): void;
    render(): any;
}
