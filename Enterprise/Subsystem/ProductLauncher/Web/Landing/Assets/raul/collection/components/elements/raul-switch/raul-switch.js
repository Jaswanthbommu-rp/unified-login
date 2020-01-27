import { h } from "@stencil/core";
export class RaulSwitch {
    constructor() {
        this.switchId = `raul-input-${switchId++}`;
        /**
         * If `true`, the switch size will be small.
         */
        this.small = false;
    }
    componentWillLoad() {
        const inputEl = this.el.querySelector('input');
        if (inputEl) {
            if (inputEl.id) {
                this.switchId = inputEl.id;
            }
            else {
                inputEl.id = this.switchId;
            }
        }
    }
    render() {
        return (h("div", { class: {
                'r-switch mb-2': true,
                'r-switch--small': this.small,
            } },
            h("slot", null),
            h("label", { class: "r-switch__label", htmlFor: this.switchId },
                h("span", { class: "r-switch__toggle" },
                    h("raul-icon", { class: "r-switch__icon", icon: "lock-1" })),
                this.labelText)));
    }
    static get is() { return "raul-switch"; }
    static get originalStyleUrls() { return {
        "$": ["raul-switch.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-switch.css"]
    }; }
    static get properties() { return {
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
                "text": "If `true`, the switch size will be small."
            },
            "attribute": "small",
            "reflect": true,
            "defaultValue": "false"
        }
    }; }
    static get states() { return {
        "switchId": {}
    }; }
    static get elementRef() { return "el"; }
}
let switchId = 0;
