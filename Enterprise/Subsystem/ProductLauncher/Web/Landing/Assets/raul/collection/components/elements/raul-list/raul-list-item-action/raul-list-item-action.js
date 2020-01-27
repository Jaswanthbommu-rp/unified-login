import { h } from "@stencil/core";
export class RaulListItemAction {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-list-item-action"; }
    static get originalStyleUrls() { return {
        "$": ["raul-list-item-action.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-list-item-action.css"]
    }; }
}
