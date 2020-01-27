import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulRadioButton {
    componentDidLoad() {
        initPage('Radio Button');
    }
    render() {
        return (h("docs-element", { title: "Heading" },
            h("div", { slot: "overview" },
                h("docs-readme", { component: "raul-heading" }),
                h("docs-showcase", null)),
            h("div", { slot: "design" },
                "Design Guidelines Stuff",
                h("docs-readme", { component: "raul-radio" })),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-radio", content: "<input type='radio' name='foo' value='bar' />" }),
                h("docs-interface", { component: "raul-radio" }))));
    }
    static get is() { return "docs-raul-radio-button"; }
}
