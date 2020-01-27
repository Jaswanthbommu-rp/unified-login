import { h } from "@stencil/core";
export class RaulFooterCopyright {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-footer-copyright"; }
}
