import { h } from "@stencil/core";
export class RaulCardHeader {
    render() {
        return [
            h("div", { class: "r-card__header__content" },
                h("slot", null)),
            h("slot", { name: "card-header-actions" })
        ];
    }
    static get is() { return "raul-card-header"; }
    static get originalStyleUrls() { return {
        "$": ["raul-card-header.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-card-header.css"]
    }; }
}
