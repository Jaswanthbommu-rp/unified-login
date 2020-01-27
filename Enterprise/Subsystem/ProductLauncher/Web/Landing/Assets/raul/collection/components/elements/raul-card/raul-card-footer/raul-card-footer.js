import { h } from "@stencil/core";
export class RaulCardFooter {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-card-footer"; }
    static get originalStyleUrls() { return {
        "$": ["raul-card-footer.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-card-footer.css"]
    }; }
}
