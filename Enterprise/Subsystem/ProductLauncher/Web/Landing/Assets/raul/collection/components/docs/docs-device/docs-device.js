import { h } from "@stencil/core";
export class DocsDevice {
    render() {
        return (h("div", { class: "r-docs-device" },
            h("div", { class: "r-docs-device-content" },
                h("slot", null))));
    }
    static get is() { return "docs-device"; }
    static get originalStyleUrls() { return {
        "$": ["docs-device.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-device.css"]
    }; }
}
