import { h } from "@stencil/core";
export class RaulAccordion {
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-accordion"; }
    static get originalStyleUrls() { return {
        "$": ["raul-accordion.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-accordion.css"]
    }; }
}
