import { h } from "@stencil/core";
export class RaulModal {
    render() {
        return (h("div", { class: 'r-modal__header__container' },
            h("div", { class: 'r-modal__header__items' },
                h("raul-content", null,
                    h("h2", { class: 'font-xl' }, this.modalTitle),
                    h("p", null, this.description))),
            h("div", { class: 'r-modal__header__close-button', role: 'button', tabindex: "0", onClick: () => this.modalClose.emit(), onKeyDown: e => { if (e.key === 'Enter')
                    this.modalClose.emit(); } },
                h("div", { class: 'r-modal__header__close-button__focus-utility', tabindex: "-1" },
                    h("raul-icon", { icon: "close" })))));
    }
    static get is() { return "raul-modal-header"; }
    static get properties() { return {
        "modalTitle": {
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
            "attribute": "modal-title",
            "reflect": false
        },
        "description": {
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
            "attribute": "description",
            "reflect": false
        }
    }; }
    static get events() { return [{
            "method": "modalClose",
            "name": "modalClose",
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
}
