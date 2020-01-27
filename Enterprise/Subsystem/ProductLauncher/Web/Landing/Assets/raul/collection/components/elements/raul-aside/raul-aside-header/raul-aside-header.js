import { h } from "@stencil/core";
export class RaulAsideHeader {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-aside-header"; }
    static get originalStyleUrls() { return {
        "$": ["raul-aside-header.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-aside-header.css"]
    }; }
}
