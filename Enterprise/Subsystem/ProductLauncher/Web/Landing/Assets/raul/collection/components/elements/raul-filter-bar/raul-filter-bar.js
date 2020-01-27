import { h } from "@stencil/core";
export class RaulFilterBar {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-filter-bar"; }
    static get originalStyleUrls() { return {
        "$": ["raul-filter-bar.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-filter-bar.css"]
    }; }
}
