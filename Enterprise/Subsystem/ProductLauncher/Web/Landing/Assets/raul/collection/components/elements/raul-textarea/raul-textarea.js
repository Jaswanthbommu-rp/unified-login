import { h } from "@stencil/core";
export class RaulTextarea {
    constructor() {
        this.inputId = `raul-textarea-${textareaIds++}`;
    }
    componentDidLoad() {
        const textareaEl = this.el.querySelector('textarea');
        if (textareaEl) {
            if (textareaEl.id) {
                this.inputId = textareaEl.id;
            }
            else {
                textareaEl.id = this.inputId;
            }
        }
    }
    render() {
        return (h("div", { class: {
                'r-form-element': true,
                'r-form-element--invalid': !!this.error
            } },
            this.label && h("label", { class: "r-form-element__label", htmlFor: this.inputId }, this.label),
            h("div", { class: "r-form-element__control" },
                h("slot", null)),
            this.hint && h("small", { class: "r-form-element__hint" }, this.hint),
            this.error &&
                h("div", { class: "r-form-element__error" },
                    h("raul-icon", { icon: "interface-alert-diamond", class: "r-form-element__error__icon" }),
                    " ",
                    this.error)));
    }
    static get is() { return "raul-textarea"; }
    static get originalStyleUrls() { return {
        "$": ["raul-textarea.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-textarea.css"]
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
                "text": "Textarea's label text."
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
                "text": "Textarea's optional hint text."
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
                "text": "Makes the textarea visually invalid and shows the error message."
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
let textareaIds = 0;
