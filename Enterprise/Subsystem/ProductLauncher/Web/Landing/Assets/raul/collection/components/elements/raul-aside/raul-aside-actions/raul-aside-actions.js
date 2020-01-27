import { h } from "@stencil/core";
export class RaulAsideActions {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-aside-actions"; }
    static get originalStyleUrls() { return {
        "$": ["raul-aside-actions.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-aside-actions.css"]
    }; }
}
