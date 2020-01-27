import { Host, h } from "@stencil/core";
export class RaulBreadcrumbs {
    constructor() {
        this.overflow = false;
    }
    render() {
        return (h(Host, { role: "list" },
            h("slot", null)));
    }
    static get is() { return "raul-breadcrumbs"; }
    static get originalStyleUrls() { return {
        "$": ["raul-breadcrumbs.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-breadcrumbs.css"]
    }; }
    static get properties() { return {
        "overflow": {
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
            "attribute": "overflow",
            "reflect": true,
            "defaultValue": "false"
        }
    }; }
}
