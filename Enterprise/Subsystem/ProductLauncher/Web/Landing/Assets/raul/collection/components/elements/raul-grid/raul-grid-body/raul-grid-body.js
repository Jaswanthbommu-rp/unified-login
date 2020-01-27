import { h } from "@stencil/core";
export class RaulGridBody {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-grid-body"; }
    static get originalStyleUrls() { return {
        "$": ["raul-grid-body.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-grid-body.css"]
    }; }
}
