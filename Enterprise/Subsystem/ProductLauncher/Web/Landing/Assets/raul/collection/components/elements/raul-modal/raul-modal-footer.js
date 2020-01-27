import { h } from "@stencil/core";
export class RaulModalFooter {
    render() {
        return (h("div", { class: 'r-modal__footer' },
            h("slot", null)));
    }
    static get is() { return "raul-modal-footer"; }
}
