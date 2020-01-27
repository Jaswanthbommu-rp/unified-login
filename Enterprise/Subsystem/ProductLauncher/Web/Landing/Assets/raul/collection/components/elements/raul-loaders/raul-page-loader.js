import { h } from "@stencil/core";
export class RaulPageLoader {
    render() {
        return (h("div", { class: 'r-page-loader__container' },
            h("div", { class: 'r-page-loader__spinner' })));
    }
    static get is() { return "raul-page-loader"; }
    static get originalStyleUrls() { return {
        "$": ["raul-page-loader.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-page-loader.css"]
    }; }
}
