import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsColors {
    componentDidLoad() {
        initPage('Colors', false);
    }
    render() {
        return (h("docs-element", { title: "Colors" },
            h("div", { slot: "overview" }, "Overview"),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" }, "API Stuff")));
    }
    static get is() { return "docs-colors"; }
}
