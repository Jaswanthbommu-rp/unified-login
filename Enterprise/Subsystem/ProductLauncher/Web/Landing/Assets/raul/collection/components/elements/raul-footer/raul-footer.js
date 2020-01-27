import { h } from "@stencil/core";
export class RaulFooter {
    render() {
        return (h("div", { class: "r-footer" },
            h("slot", null)));
    }
    static get is() { return "raul-footer"; }
    static get originalStyleUrls() { return {
        "$": ["raul-footer.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-footer.css"]
    }; }
}
