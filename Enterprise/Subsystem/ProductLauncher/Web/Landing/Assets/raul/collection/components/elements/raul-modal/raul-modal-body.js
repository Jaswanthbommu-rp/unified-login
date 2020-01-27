import { h } from "@stencil/core";
export class RaulModalBody {
    render() {
        return (h("div", { class: "r-modal__body__container" },
            h("slot", null)));
    }
    static get is() { return "raul-modal-body"; }
}
