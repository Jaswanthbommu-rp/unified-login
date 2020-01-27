import { h } from "@stencil/core";
export class RaulAvatar {
    constructor() {
        this.variant = 'profile';
    }
    render() {
        return (h("raul-icon", { icon: this.variant === 'profile' ? 'user' : 'building-7', class: "raul-avatar__icon" }));
    }
    static get is() { return "raul-avatar"; }
    static get originalStyleUrls() { return {
        "$": ["raul-avatar.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-avatar.css"]
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
        "primary": {
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
            "attribute": "primary",
            "reflect": true
        },
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'profile' | 'property'",
                "resolved": "\"profile\" | \"property\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "variant",
            "reflect": true,
            "defaultValue": "'profile'"
        }
    }; }
}
