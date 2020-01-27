import { h } from "@stencil/core";
export class RaulList {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-list"; }
    static get originalStyleUrls() { return {
        "$": ["raul-list.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-list.css"]
    }; }
}
