import { Host, h } from "@stencil/core";
export class RaulListHeader {
    render() {
        return (h(Host, null,
            h("slot", null)));
    }
    static get is() { return "raul-list-header"; }
    static get originalStyleUrls() { return {
        "$": ["raul-list-header.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-list-header.css"]
    }; }
}
