import { h } from "@stencil/core";
export class RaulAsideFooter {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-aside-footer"; }
    static get originalStyleUrls() { return {
        "$": ["raul-aside-footer.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-aside-footer.css"]
    }; }
}
