import { h } from "@stencil/core";
export class RaulStatusIndicator {
    constructor() {
        this.variant = 'default';
    }
    render() {
        return (h("div", { class: {
                'status-indicator': true,
                [`status-indicator--${this.variant}`]: this.variant !== 'default'
            } }));
    }
    static get is() { return "raul-status-indicator"; }
    static get originalStyleUrls() { return {
        "$": ["raul-status-indicator.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-status-indicator.css"]
    }; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'default' | 'success' | 'warning' | 'destructive'",
                "resolved": "\"default\" | \"destructive\" | \"success\" | \"warning\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "variant",
            "reflect": false,
            "defaultValue": "'default'"
        }
    }; }
}
