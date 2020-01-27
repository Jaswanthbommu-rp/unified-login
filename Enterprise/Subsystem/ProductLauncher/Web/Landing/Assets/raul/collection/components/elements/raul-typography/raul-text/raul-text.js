import { h } from "@stencil/core";
import { randomUID } from '../../../../utils/string';
export class RaulText {
    constructor() {
        this.align = 'left';
        this.inline = false;
        this.strong = false;
        this.emphasis = false;
        this.underline = false;
        this.lineThrough = false;
        this.ellipsis = false;
        this.capitalize = false;
        this.uppercase = false;
        this.paragraph = false;
    }
    render() {
        const Tag = this.inline ? 'span' :
            this.size === 'small' ? 'p' :
                this.size === 'medium' ? 'p' :
                    this.size === 'large' ? 'h3' :
                        this.size === 'extra-large' ? 'h2' :
                            this.size === 'hero' ? 'h1'
                                : 'span';
        return (h(Tag, { class: {
                ['r-text']: true,
                [`r-text--${this.size}`]: !!this.size,
                [`r-text--${this.color}`]: !!this.color,
                [`r-text--align-${this.align}`]: true,
                ['r-text--inline']: this.inline,
                ['r-text--strong']: this.strong,
                ['r-text--underline']: this.underline,
                ['r-text--line-through']: this.lineThrough,
                ['r-text--emphasis']: this.emphasis,
                ['r-text--ellipsis']: this.ellipsis,
                ['r-text--capitalize']: this.capitalize,
                ['r-text--uppercase']: this.uppercase,
                ['r-text--paragraph']: this.paragraph
            }, key: randomUID() },
            h("slot", null)));
    }
    static get is() { return "raul-text"; }
    static get originalStyleUrls() { return {
        "$": ["raul-text.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-text.css"]
    }; }
    static get properties() { return {
        "size": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'small' | 'medium' | 'large' | 'extra-large' | 'hero'",
                "resolved": "\"extra-large\" | \"hero\" | \"large\" | \"medium\" | \"small\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "size",
            "reflect": false
        },
        "align": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'left' | 'center' | 'right' | 'justify'",
                "resolved": "\"center\" | \"justify\" | \"left\" | \"right\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "align",
            "reflect": false,
            "defaultValue": "'left'"
        },
        "color": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'primary' | 'success' | 'danger' | 'white'",
                "resolved": "\"danger\" | \"primary\" | \"success\" | \"white\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "color",
            "reflect": false
        },
        "inline": {
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
            "attribute": "inline",
            "reflect": false,
            "defaultValue": "false"
        },
        "strong": {
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
            "attribute": "strong",
            "reflect": false,
            "defaultValue": "false"
        },
        "emphasis": {
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
            "attribute": "emphasis",
            "reflect": false,
            "defaultValue": "false"
        },
        "underline": {
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
            "attribute": "underline",
            "reflect": false,
            "defaultValue": "false"
        },
        "lineThrough": {
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
            "attribute": "line-through",
            "reflect": false,
            "defaultValue": "false"
        },
        "ellipsis": {
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
            "attribute": "ellipsis",
            "reflect": false,
            "defaultValue": "false"
        },
        "capitalize": {
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
            "attribute": "capitalize",
            "reflect": false,
            "defaultValue": "false"
        },
        "uppercase": {
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
            "attribute": "uppercase",
            "reflect": false,
            "defaultValue": "false"
        },
        "paragraph": {
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
            "attribute": "paragraph",
            "reflect": false,
            "defaultValue": "false"
        }
    }; }
}
