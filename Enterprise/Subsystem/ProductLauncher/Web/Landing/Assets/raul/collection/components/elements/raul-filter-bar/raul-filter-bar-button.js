import { h } from "@stencil/core";
import { ripple } from '../../../utils/animation';
export class RaulFilterBarButton {
    constructor() {
        this.filterCount = 0;
    }
    render() {
        return (h("div", { class: "r-filter-bar-button" },
            h("div", { class: "r-filter-bar-button__focus-ring", ref: el => this.focusRingEl = el }),
            h("button", { class: "r-filter-bar-button__element", type: "button", onMouseDown: e => ripple(e, this.focusRingEl) }),
            h("div", { class: "r-filter-bar-button__content" },
                h("raul-icon", { icon: "content-filter" }),
                this.filterCount > 0 && `(${this.filterCount})`,
                " Filters")));
    }
    static get is() { return "raul-filter-bar-button"; }
    static get originalStyleUrls() { return {
        "$": ["raul-filter-bar.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-filter-bar.css"]
    }; }
    static get properties() { return {
        "filterCount": {
            "type": "number",
            "mutable": false,
            "complexType": {
                "original": "number",
                "resolved": "number",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "filter-count",
            "reflect": true,
            "defaultValue": "0"
        }
    }; }
}
