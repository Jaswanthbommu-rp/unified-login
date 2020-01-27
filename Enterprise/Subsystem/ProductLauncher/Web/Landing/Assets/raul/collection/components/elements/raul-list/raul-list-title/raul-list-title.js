import { h } from "@stencil/core";
export class RaulListTitle {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-list-title"; }
    static get originalStyleUrls() { return {
        "$": ["raul-list-title.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-list-title.css"]
    }; }
}
