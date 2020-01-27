import { h } from "@stencil/core";
export class RaulGridFooter {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-grid-footer"; }
    static get originalStyleUrls() { return {
        "$": ["raul-grid-footer.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-grid-footer.css"]
    }; }
}
