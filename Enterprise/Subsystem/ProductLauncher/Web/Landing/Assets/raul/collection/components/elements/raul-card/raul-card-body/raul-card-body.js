import { h } from "@stencil/core";
export class RaulCardBody {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-card-body"; }
    static get originalStyleUrls() { return {
        "$": ["raul-card-body.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-card-body.css"]
    }; }
}
