import { h } from "@stencil/core";
export class RaulAsideBody {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-aside-body"; }
    static get originalStyleUrls() { return {
        "$": ["raul-aside-body.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-aside-body.css"]
    }; }
}
