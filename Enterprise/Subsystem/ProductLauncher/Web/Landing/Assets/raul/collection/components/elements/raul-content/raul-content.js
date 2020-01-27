import { h } from "@stencil/core";
export class RaulContent {
    constructor() {
        this.fullWidth = false;
    }
    render() {
        return (h("div", { class: {
                'r-content': true,
                'r-content--fullwidth': this.fullWidth
            } },
            h("slot", null)));
    }
    static get is() { return "raul-content"; }
    static get originalStyleUrls() { return {
        "$": ["raul-content.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-content.css"]
    }; }
    static get properties() { return {
        "fullWidth": {
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
            "attribute": "full-width",
            "reflect": false,
            "defaultValue": "false"
        }
    }; }
}
