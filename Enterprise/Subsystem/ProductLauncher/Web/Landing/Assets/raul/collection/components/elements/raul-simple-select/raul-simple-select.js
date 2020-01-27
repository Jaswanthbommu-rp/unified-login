import { h } from "@stencil/core";
export class RaulSimpleSelect {
    constructor() {
        this.inputId = `raul-select-${selectIds++}`;
    }
    render() {
        return (h("div", { class: {
                'r-form-element': true,
                'r-form-element--invalid': !!this.error
            } },
            this.label && h("label", { class: "r-form-element__label", htmlFor: this.inputId }, this.label),
            h("div", { class: "r-form-element__control" },
                h("slot", null),
                h("raul-icon", { class: "r-form-element__control__arrow", icon: "arrow-down-v" })),
            this.hint && h("small", { class: "r-form-element__hint" }, this.hint),
            this.error &&
                h("div", { class: "r-form-element__error" },
                    h("raul-icon", { icon: "interface-alert-diamond", class: "r-form-element__error__icon" }),
                    " ",
                    this.error)));
    }
    static get is() { return "raul-simple-select"; }
    static get originalStyleUrls() { return {
        "$": ["raul-simple-select.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-simple-select.css"]
    }; }
    static get properties() { return {
        "label": {
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
                "text": "Input's label text."
            },
            "attribute": "label",
            "reflect": false
        },
        "hint": {
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
                "text": "Input's optional hint text."
            },
            "attribute": "hint",
            "reflect": false
        },
        "error": {
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
                "text": "Makes the input visually invalid and shows the feedback message."
            },
            "attribute": "error",
            "reflect": false
        }
    }; }
    static get states() { return {
        "inputId": {}
    }; }
    static get elementRef() { return "el"; }
}
let selectIds = 0;
