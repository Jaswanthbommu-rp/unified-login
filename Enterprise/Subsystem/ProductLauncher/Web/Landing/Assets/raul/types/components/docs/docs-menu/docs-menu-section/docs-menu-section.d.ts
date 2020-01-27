import { EventEmitter } from '../../../../stencil.core';
export declare class DocsMenuSection {
    el: HTMLElement;
    contentEl: HTMLElement;
    name: String;
    url: string;
    expanded: boolean;
    expandSection: EventEmitter;
    componentDidLoad(): void;
    setInitialToggleState(): void;
    handleClick(e: any): void;
    maybeCollapse(e: any): void;
    renderSectionTitle(): any;
    render(): any;
}
