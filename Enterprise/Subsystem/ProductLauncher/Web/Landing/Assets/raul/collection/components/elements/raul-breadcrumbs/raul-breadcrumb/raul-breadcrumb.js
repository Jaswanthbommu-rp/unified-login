import { Host, h } from "@stencil/core";
export class RaulBreadcrumb {
    render() {
        return (h(Host, { role: "listitem" },
            h("slot", null),
            h("raul-icon", { icon: "arrow-right-v", class: "r-breadcrumb__separator-icon" })));
    }
    static get is() { return "raul-breadcrumb"; }
    static get originalStyleUrls() { return {
        "$": ["raul-breadcrumb.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-breadcrumb.css"]
    }; }
}
