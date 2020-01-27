import { EventEmitter } from '../../../../stencil.core';
export declare class RaulAccordionItem {
    private headerEl;
    private panelEl;
    private accordionId;
    expandedState: boolean;
    el: HTMLRaulAccordionItemElement;
    name: string;
    expanded: boolean;
    disabled: boolean;
    nameChanged(): void;
    expandedChanged(): void;
    disabledChanged(): void;
    componentDidLoad(): void;
    handleAccordionItemHeaderClick(): void;
    raulChange: EventEmitter;
    private isControlled;
    private updateHeaderElName;
    private updateHeaderElExpanded;
    private updateHeaderElDisabled;
    private updatePanelElName;
    private updatePanelElExpanded;
    render(): any;
}
