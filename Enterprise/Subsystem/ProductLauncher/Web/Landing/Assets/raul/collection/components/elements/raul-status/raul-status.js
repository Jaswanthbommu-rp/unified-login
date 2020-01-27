import { h } from "@stencil/core";
export class RaulChip {
    constructor() {
        /**
         * Status variant.
         */
        this.variant = 'default';
    }
    componentDidLoad() {
        this.title = this.statusTextEl.textContent;
    }
    render() {
        return (h("div", { class: {
                'status': true,
                [`status--${this.variant}`]: this.variant !== 'default'
            } },
            h("div", { class: "status__text", title: this.title, ref: el => this.statusTextEl = el },
                h("slot", null))));
    }
    static get is() { return "raul-status"; }
    static get originalStyleUrls() { return {
        "$": ["raul-status.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-status.css"]
    }; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'default' | 'destructive' | 'warning' | 'success'",
                "resolved": "\"default\" | \"destructive\" | \"success\" | \"warning\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Status variant."
            },
            "attribute": "variant",
            "reflect": false,
            "defaultValue": "'default'"
        }
    }; }
    static get states() { return {
        "title": {}
    }; }
}
