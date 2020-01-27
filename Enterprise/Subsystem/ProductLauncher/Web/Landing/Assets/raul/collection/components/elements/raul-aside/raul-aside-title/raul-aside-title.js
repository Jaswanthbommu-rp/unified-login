import { h } from "@stencil/core";
export class RaulAsideTitle {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-aside-title"; }
    static get originalStyleUrls() { return {
        "$": ["raul-aside-title.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-aside-title.css"]
    }; }
}
