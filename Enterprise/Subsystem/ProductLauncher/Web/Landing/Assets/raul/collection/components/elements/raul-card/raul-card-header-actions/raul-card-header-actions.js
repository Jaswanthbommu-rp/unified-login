import { h } from "@stencil/core";
export class RaulCardHeaderActions {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-card-header-actions"; }
    static get originalStyleUrls() { return {
        "$": ["raul-card-header-actions.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-card-header-actions.css"]
    }; }
}
