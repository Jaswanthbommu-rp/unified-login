import { h } from "@stencil/core";
export class RaulListItemSubtitle {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-list-item-subtitle"; }
    static get originalStyleUrls() { return {
        "$": ["raul-list-item-subtitle.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-list-item-subtitle.css"]
    }; }
}
