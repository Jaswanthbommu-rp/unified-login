import { h } from "@stencil/core";
import { randomUID } from '../../../../utils/string';
export class RaulHeading {
    render() {
        const size = this.variant === 'page' ? 'hero' :
            this.variant === 'section' ? 'extra-large' :
                this.variant === 'content' ? 'large'
                    : 'large';
        return (h("raul-text", { paragraph: true, size: size, key: randomUID() },
            h("slot", null)));
    }
    static get is() { return "raul-heading"; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'page' | 'section' | 'content'",
                "resolved": "\"content\" | \"page\" | \"section\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "variant",
            "reflect": false
        }
    }; }
}
