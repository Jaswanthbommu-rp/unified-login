import { Host, h } from "@stencil/core";
export class RaulAvatar {
    constructor() {
        this.static = false;
        this.label = null;
        this.hint = null;
        this.value = '0';
        this.color = 'primary';
    }
    handleProgressRemove() {
        this.raulProgressRemove.emit();
    }
    render() {
        return (h(Host, null,
            this.label &&
                h("div", { class: "r-progress__label" },
                    this.label,
                    !this.static &&
                        h("button", { type: "button", class: "r-progress__remove-btn", onClick: () => this.handleProgressRemove() },
                            h("raul-icon", { icon: "remove-2" }))),
            h("div", { class: "r-progress__bar" },
                h("div", { class: "r-progress__bar__value", style: { width: `${this.value}%` } })),
            h("div", { class: "r-progress__text" },
                `${this.value}%`,
                " ",
                !this.static && this.hint)));
    }
    static get is() { return "raul-progress"; }
    static get originalStyleUrls() { return {
        "$": ["raul-progress.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-progress.css"]
    }; }
    static get properties() { return {
        "static": {
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
            "attribute": "static",
            "reflect": true,
            "defaultValue": "false"
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
                "text": ""
            },
            "attribute": "label",
            "reflect": true,
            "defaultValue": "null"
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
                "text": ""
            },
            "attribute": "hint",
            "reflect": true,
            "defaultValue": "null"
        },
        "value": {
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
            "attribute": "value",
            "reflect": true,
            "defaultValue": "'0'"
        },
        "color": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'primary' | 'warning' | 'danger' | 'success'",
                "resolved": "\"danger\" | \"primary\" | \"success\" | \"warning\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "color",
            "reflect": true,
            "defaultValue": "'primary'"
        }
    }; }
    static get events() { return [{
            "method": "raulProgressRemove",
            "name": "raulProgressRemove",
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
