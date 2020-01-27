import { h } from "@stencil/core";
export class RaulBreadcrumbBackIcon {
    render() {
        return (h("raul-icon", { icon: "arrow-left-1" }));
    }
    static get is() { return "raul-breadcrumb-back-icon"; }
    static get originalStyleUrls() { return {
        "$": ["raul-breadcrumb-back-icon.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-breadcrumb-back-icon.css"]
    }; }
}
