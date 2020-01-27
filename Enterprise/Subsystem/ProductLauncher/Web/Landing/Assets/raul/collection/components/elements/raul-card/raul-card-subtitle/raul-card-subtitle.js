import { h } from "@stencil/core";
export class RaulCardSubtitle {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-card-subtitle"; }
    static get originalStyleUrls() { return {
        "$": ["raul-card-subtitle.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-card-subtitle.css"]
    }; }
}
