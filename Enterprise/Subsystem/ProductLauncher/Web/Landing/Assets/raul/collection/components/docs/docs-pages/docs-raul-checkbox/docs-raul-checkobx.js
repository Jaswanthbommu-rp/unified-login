import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulCheckobx {
    componentDidLoad() {
        initPage('Checkbox');
    }
    render() {
        return (h("docs-element", { title: "Checkbox" },
            h("div", { slot: "overview" },
                h("docs-showcase", null)),
            h("div", { slot: "design" },
                h("docs-readme", { component: "raul-checkbox" })),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-checkbox", content: "<input type='checkbox' name='foo' value='bar' />" }),
                h("docs-interface", { component: "raul-checkbox" }))));
    }
    static get is() { return "docs-raul-checkbox"; }
}
