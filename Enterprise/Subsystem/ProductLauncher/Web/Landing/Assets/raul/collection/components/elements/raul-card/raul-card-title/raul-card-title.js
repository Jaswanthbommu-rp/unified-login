import { h } from "@stencil/core";
export class RaulCardTitle {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-card-title"; }
    static get originalStyleUrls() { return {
        "$": ["raul-card-title.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-card-title.css"]
    }; }
}
