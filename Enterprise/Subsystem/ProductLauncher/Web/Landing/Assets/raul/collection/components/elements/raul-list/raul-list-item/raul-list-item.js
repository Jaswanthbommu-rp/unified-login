import { Host, h } from "@stencil/core";
export class RaulListItem {
    render() {
        return (h(Host, { role: "listitem" },
            h("slot", null)));
    }
    static get is() { return "raul-list-item"; }
    static get originalStyleUrls() { return {
        "$": ["raul-list-item.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-list-item.css"]
    }; }
}
