import { h } from "@stencil/core";
export class RaulGridRow {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-grid-row"; }
    static get originalStyleUrls() { return {
        "$": ["raul-grid-row.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-grid-row.css"]
    }; }
}
