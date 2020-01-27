import { h } from "@stencil/core";
export class RaulSimpleTable {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-simple-table"; }
    static get originalStyleUrls() { return {
        "$": ["raul-simple-table.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-simple-table.css"]
    }; }
    static get properties() { return {
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
                "text": ""
            },
            "attribute": "small",
            "reflect": true
        },
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
            "reflect": true
        },
        "striped": {
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
            "attribute": "striped",
            "reflect": true
        }
    }; }
}
