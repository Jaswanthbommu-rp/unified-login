import { h } from "@stencil/core";
export class RaulAlert {
    constructor() {
        /**
         * Determines the color of the left bar
         */
        this.variant = 'information';
        /**
         * Corners can be rounded or not
         */
        this.rounded = false;
    }
    componentWillLoad() {
        if (!this.heading) {
            console.error('Heading text is required for Raul alert component');
        }
    }
    render() {
        return (h("div", { class: {
                'r-alert': true,
                [`r-alert__${this.variant}`]: true,
                ['r-alert__rounded']: this.rounded
            } },
            h("div", { class: 'r-alert__heading' }, this.heading),
            this.content &&
                h("div", { class: 'r-alert__content' }, this.content),
            this.ctaMessage && this.ctaUrl ?
                h("a", { href: this.ctaUrl },
                    h("div", { class: 'r-alert__cta' }, this.ctaMessage)) : null));
    }
    static get is() { return "raul-alert"; }
    static get originalStyleUrls() { return {
        "$": ["raul-alert.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-alert.css"]
    }; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'information' | 'success' | 'warning' | 'danger'",
                "resolved": "\"danger\" | \"information\" | \"success\" | \"warning\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Determines the color of the left bar"
            },
            "attribute": "variant",
            "reflect": false,
            "defaultValue": "'information'"
        },
        "heading": {
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
                "text": "A header"
            },
            "attribute": "heading",
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
            "optional": true,
            "docs": {
                "tags": [],
                "text": "Alert text"
            },
            "attribute": "content",
            "reflect": false
        },
        "ctaMessage": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": "Action link text"
            },
            "attribute": "cta-message",
            "reflect": false
        },
        "ctaUrl": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": "An action url"
            },
            "attribute": "cta-url",
            "reflect": false
        },
        "rounded": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": "Corners can be rounded or not"
            },
            "attribute": "rounded",
            "reflect": false,
            "defaultValue": "false"
        }
    }; }
}
