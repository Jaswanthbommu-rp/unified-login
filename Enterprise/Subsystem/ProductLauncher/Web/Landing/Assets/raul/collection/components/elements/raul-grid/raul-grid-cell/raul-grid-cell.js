import { h } from "@stencil/core";
export class RaulGridCell {
    render() {
        return (h("div", { class: "r-grid__cell" },
            h("slot", null)));
    }
    static get is() { return "raul-grid-cell"; }
    static get originalStyleUrls() { return {
        "$": ["raul-grid-cell.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-grid-cell.css"]
    }; }
}
