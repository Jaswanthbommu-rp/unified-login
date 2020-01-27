import { h } from "@stencil/core";
export class RaulAccordionItem {
    constructor() {
        this.accordionId = accordionIds++;
        this.expandedState = false;
        this.name = `accordion-${this.accordionId}`;
        this.expanded = null;
        this.disabled = false;
    }
    nameChanged() {
        this.updateHeaderElName();
        this.updatePanelElName();
    }
    expandedChanged() {
        this.updateHeaderElExpanded();
        this.updatePanelElExpanded();
    }
    disabledChanged() {
        this.updateHeaderElDisabled();
    }
    componentDidLoad() {
        this.headerEl = this.el.querySelector('raul-accordion-item-header');
        this.panelEl = this.el.querySelector('raul-accordion-item-panel');
        this.updateHeaderElName();
        this.updateHeaderElExpanded();
        this.updateHeaderElDisabled();
        this.updatePanelElName();
        this.updatePanelElExpanded();
    }
    handleAccordionItemHeaderClick() {
        if (!this.isControlled()) {
            this.expandedState = !this.expandedState;
            this.updateHeaderElExpanded();
            this.updatePanelElExpanded();
        }
        this.raulChange.emit({ name: this.name, expanded: this.isControlled() ? !this.expanded : this.expandedState });
    }
    isControlled() {
        return this.expanded !== null;
    }
    updateHeaderElName() {
        if (this.headerEl)
            this.headerEl.name = this.name;
    }
    updateHeaderElExpanded() {
        if (this.headerEl)
            this.headerEl.expanded = this.isControlled() ? this.expanded : this.expandedState;
    }
    updateHeaderElDisabled() {
        if (this.headerEl)
            this.headerEl.disabled = this.disabled;
    }
    updatePanelElName() {
        if (this.panelEl)
            this.panelEl.name = this.name;
    }
    updatePanelElExpanded() {
        if (this.panelEl)
            this.panelEl.expanded = this.isControlled() ? this.expanded : this.expandedState;
    }
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-accordion-item"; }
    static get properties() { return {
        "name": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "name",
            "reflect": true,
            "defaultValue": "`accordion-${this.accordionId}`"
        },
        "expanded": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "expanded",
            "reflect": true,
            "defaultValue": "null"
        },
        "disabled": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "disabled",
            "reflect": true,
            "defaultValue": "false"
        }
    }; }
    static get states() { return {
        "expandedState": {}
    }; }
    static get events() { return [{
            "method": "raulChange",
            "name": "raulChange",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
    static get elementRef() { return "el"; }
    static get watchers() { return [{
            "propName": "name",
            "methodName": "nameChanged"
        }, {
            "propName": "expanded",
            "methodName": "expandedChanged"
        }, {
            "propName": "disabled",
            "methodName": "disabledChanged"
        }]; }
    static get listeners() { return [{
            "name": "accordionItemHeaderClick",
            "method": "handleAccordionItemHeaderClick",
            "target": undefined,
            "capture": false,
            "passive": false
        }]; }
}
let accordionIds = 0;
