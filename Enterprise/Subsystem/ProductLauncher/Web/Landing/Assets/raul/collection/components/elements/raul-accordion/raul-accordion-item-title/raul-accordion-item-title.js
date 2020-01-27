import { h } from "@stencil/core";
export class RaulAccordionItemTitle {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-accordion-item-title"; }
}
