import { h } from "@stencil/core";
export class RaulListItemTitle {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-list-item-title"; }
    static get originalStyleUrls() { return {
        "$": ["raul-list-item-title.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-list-item-title.css"]
    }; }
}
