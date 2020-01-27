import { h } from "@stencil/core";
export class RaulAsideSubtitle {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-aside-subtitle"; }
    static get originalStyleUrls() { return {
        "$": ["raul-aside-subtitle.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-aside-subtitle.css"]
    }; }
}
