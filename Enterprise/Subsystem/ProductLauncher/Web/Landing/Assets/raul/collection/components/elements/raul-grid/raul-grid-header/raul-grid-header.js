import { h } from "@stencil/core";
export class RaulGridHeader {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-grid-header"; }
    static get originalStyleUrls() { return {
        "$": ["raul-grid-header.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-grid-header.css"]
    }; }
}
