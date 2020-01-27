import { h } from "@stencil/core";
export class RaulBadge {
    constructor() {
        /**
         * Determines the primary appearance of the badge based on its purpose.
         */
        this.variant = 'primary';
    }
    render() {
        return (h("div", { class: `r-badge__container r-badge__container--${this.variant}` },
            this.icon &&
                h("div", { class: 'r-badge__icon' },
                    h("raul-icon", { icon: this.icon })),
            h("div", { class: 'r-badge__content' }, this.content)));
    }
    static get is() { return "raul-badge"; }
    static get originalStyleUrls() { return {
        "$": ["raul-badge.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-badge.css"]
    }; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'primary' | 'warning' | 'error' | 'success'",
                "resolved": "\"error\" | \"primary\" | \"success\" | \"warning\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Determines the primary appearance of the badge based on its purpose."
            },
            "attribute": "variant",
            "reflect": false,
            "defaultValue": "'primary'"
        },
        "icon": {
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
                "text": "The icon to display to the left of the content."
            },
            "attribute": "icon",
            "reflect": false
        },
        "content": {
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
                "text": "The text or number to display in the badge."
            },
            "attribute": "content",
            "reflect": false
        }
    }; }
}
