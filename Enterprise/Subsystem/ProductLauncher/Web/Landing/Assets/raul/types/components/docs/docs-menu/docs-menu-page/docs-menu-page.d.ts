import { EventEmitter } from '../../../../stencil.core';
export declare class DocsMenuPage {
    el: HTMLElement;
    anchorsEl: HTMLElement;
    name: string;
    url: string;
    menuPageActivated: EventEmitter;
    handleClick(e: any): void;
    render(): any;
}
