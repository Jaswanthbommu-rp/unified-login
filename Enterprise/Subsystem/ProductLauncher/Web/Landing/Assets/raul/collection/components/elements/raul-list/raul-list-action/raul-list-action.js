import { h } from "@stencil/core";
export class RaulListAction {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-list-action"; }
    static get originalStyleUrls() { return {
        "$": ["raul-list-action.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-list-action.css"]
    }; }
}
