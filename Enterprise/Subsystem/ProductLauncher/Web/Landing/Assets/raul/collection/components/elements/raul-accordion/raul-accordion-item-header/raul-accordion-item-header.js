import { h } from "@stencil/core";
export class RaulAccordionItemHeader {
    handleClick() {
        const panelEl = this.el.parentElement.querySelector('raul-accordion-item-panel');
        if (panelEl) {
            if (!panelEl.classList.contains('expanding') && !panelEl.classList.contains('collapsing')) {
                this.accordionItemHeaderClick.emit();
            }
        }
        else {
            this.accordionItemHeaderClick.emit();
        }
    }
    render() {
        return (h("button", { type: "button", class: "r-accordion__item__header", id: `${this.name}-header`, "aria-controls": `${this.name}-panel`, "aria-expanded": this.expanded, "aria-disabled": this.disabled, onClick: () => this.handleClick() },
            h("div", { class: "r-accordion__item__header__content" },
                h("slot", null)),
            h("raul-icon", { icon: "arrow-down-v", class: "r-accordion__item__header__arrow-icon" })));
    }
    static get is() { return "raul-accordion-item-header"; }
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
            "reflect": true
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
            "reflect": true
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
            "reflect": true
        }
    }; }
    static get events() { return [{
            "method": "accordionItemHeaderClick",
            "name": "accordionItemHeaderClick",
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
}
