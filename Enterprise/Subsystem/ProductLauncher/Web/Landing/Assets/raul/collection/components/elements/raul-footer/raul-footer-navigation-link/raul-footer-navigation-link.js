import { h } from "@stencil/core";
export class RaulFooterNavigationLink {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-footer-navigation-link"; }
}
