import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsUtils {
    componentDidLoad() {
        initPage('Spacing', false);
    }
    render() {
        return (h("docs-element", { title: "Spacing" },
            h("div", { slot: "overview" }, "Overview"),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" }, "API Stuff")));
    }
    static get is() { return "docs-spacing"; }
}
