import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulheading {
    componentDidLoad() {
        initPage('heading');
    }
    render() {
        return (h("docs-element", { title: "Heading" },
            h("div", { slot: "overview" },
                h("docs-readme", { component: "raul-heading" })),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-heading", content: "Lorem ipsum dolor sit amet" }),
                h("docs-showcase", null),
                h("docs-interface", { component: "raul-heading" }))));
    }
    static get is() { return "docs-raul-heading"; }
}
