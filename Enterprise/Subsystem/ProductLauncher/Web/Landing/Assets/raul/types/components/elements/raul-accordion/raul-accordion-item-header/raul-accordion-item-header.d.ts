import { EventEmitter } from '../../../../stencil.core';
export declare class RaulAccordionItemHeader {
    el: HTMLRaulAccordionItemHeaderElement;
    name: string;
    expanded: boolean;
    disabled: boolean;
    private handleClick;
    accordionItemHeaderClick: EventEmitter;
    render(): any;
}
