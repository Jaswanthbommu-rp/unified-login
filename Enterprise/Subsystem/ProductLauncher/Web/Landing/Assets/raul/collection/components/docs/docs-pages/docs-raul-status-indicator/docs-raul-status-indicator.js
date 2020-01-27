import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulStatusIndicator {
    componentDidLoad() {
        initPage('Status Indicator');
    }
    render() {
        return (h("docs-element", { title: "Status Indicator" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("div", { class: "flex flex-wrap -mx-2" },
                        h("div", { class: "flex-none px-2" },
                            h("raul-status-indicator", null)),
                        h("div", { class: "flex-none px-2" },
                            h("raul-status-indicator", { variant: "destructive" })),
                        h("div", { class: "flex-none px-2" },
                            h("raul-status-indicator", { variant: "success" })),
                        h("div", { class: "flex-none px-2" },
                            h("raul-status-indicator", { variant: "warning" }))))),
            h("div", { slot: "design" },
                h("docs-readme", { component: "raul-status-indicator" })),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-status-indicator", content: "Status Indicator" }),
                h("docs-interface", { component: "raul-status-indicator" }))));
    }
    static get is() { return "docs-raul-status-indicator"; }
}
