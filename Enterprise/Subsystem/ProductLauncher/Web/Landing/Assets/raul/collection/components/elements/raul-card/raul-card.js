import { h } from "@stencil/core";
export class RaulCard {
    constructor() {
        this.hoverable = false;
    }
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-card"; }
    static get originalStyleUrls() { return {
        "$": ["raul-card.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-card.css"]
    }; }
    static get properties() { return {
        "hoverable": {
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
            "attribute": "hoverable",
            "reflect": true,
            "defaultValue": "false"
        }
    }; }
}
