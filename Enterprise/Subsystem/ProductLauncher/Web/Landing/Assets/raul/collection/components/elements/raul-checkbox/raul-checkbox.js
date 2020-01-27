import { h } from "@stencil/core";
export class RaulCheckbox {
    constructor() {
        /**
         * If `true`, the checkbox border will become red. This can be useful for form validations.
         */
        this.invalid = false;
        /**
         * If `true`, the checkbox size will be small.
         */
        this.small = false;
    }
    render() {
        return (h("div", { class: {
                'r-checkbox': true,
                'r-checkbox--invalid': this.invalid,
                'r-checkbox--small': this.small
            } },
            h("label", { class: "r-checkbox__label" },
                h("slot", null),
                h("div", { class: "r-checkbox__label__icon" }),
                this.labelText &&
                    h("div", { class: "r-checkbox__label__text" }, this.labelText))));
    }
    static get is() { return "raul-checkbox"; }
    static get originalStyleUrls() { return {
        "$": ["raul-checkbox.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-checkbox.css"]
    }; }
    static get properties() { return {
        "invalid": {
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
                "text": "If `true`, the checkbox border will become red. This can be useful for form validations."
            },
            "attribute": "invalid",
            "reflect": false,
            "defaultValue": "false"
        },
        "labelText": {
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
                "text": "The text label."
            },
            "attribute": "label-text",
            "reflect": false
        },
        "small": {
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
                "text": "If `true`, the checkbox size will be small."
            },
            "attribute": "small",
            "reflect": true,
            "defaultValue": "false"
        }
    }; }
    static get elementRef() { return "el"; }
}
