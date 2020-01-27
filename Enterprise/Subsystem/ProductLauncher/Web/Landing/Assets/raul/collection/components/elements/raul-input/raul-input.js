import { h } from "@stencil/core";
export class RaulInput {
    constructor() {
        this.inputId = `raul-input-${inputIds++}`;
        /**
         * Input's type.
         */
        this.type = 'text';
    }
    componentDidLoad() {
        const inputEl = this.el.querySelector('input');
        if (inputEl) {
            if (inputEl.id) {
                this.inputId = inputEl.id;
            }
            else {
                inputEl.id = this.inputId;
            }
        }
    }
    render() {
        return (h("div", { class: {
                'r-form-element': true,
                'r-form-element--invalid': !!this.error,
                'r-form-element--search': this.type === 'search'
            } },
            this.label && h("label", { class: "r-form-element__label", htmlFor: this.inputId }, this.label),
            h("div", { class: "r-form-element__control" },
                h("slot", null),
                this.type === 'search' && h("raul-icon", { icon: "search", class: "r-from-element__control__search-icon" })),
            this.hint && h("small", { class: "r-form-element__hint" }, this.hint),
            this.error &&
                h("div", { class: "r-form-element__error" },
                    h("raul-icon", { icon: "interface-alert-diamond", class: "r-form-element__error__icon" }),
                    " ",
                    this.error)));
    }
    static get is() { return "raul-input"; }
    static get originalStyleUrls() { return {
        "$": ["raul-input.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-input.css"]
    }; }
    static get properties() { return {
        "type": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'text' | 'search'",
                "resolved": "\"search\" | \"text\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Input's type."
            },
            "attribute": "type",
            "reflect": false,
            "defaultValue": "'text'"
        },
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
let inputIds = 0;
