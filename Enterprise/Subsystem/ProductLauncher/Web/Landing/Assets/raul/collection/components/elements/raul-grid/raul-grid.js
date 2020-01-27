import { h } from "@stencil/core";
export class RaulGrid {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-grid"; }
    static get originalStyleUrls() { return {
        "$": ["raul-grid.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-grid.css"]
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
