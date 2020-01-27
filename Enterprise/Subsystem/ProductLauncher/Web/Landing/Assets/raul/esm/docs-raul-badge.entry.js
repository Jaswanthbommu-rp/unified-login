import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsRaulBadge = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Badge');
    }
    render() {
        return (h("docs-element", { title: "Badges" }, h("div", { slot: "overview", class: "r-content" }, h("docs-showcase", null, h("p", { class: "font-md" }, h("raul-badge", { icon: "add-circle-1", content: "88" }), " ", h("code", null, "variant=\"primary\"")), h("p", { class: "font-md" }, h("raul-badge", { icon: "add-circle-1", content: "88", variant: "error" }), " ", h("code", null, "variant=\"error\"")), h("p", { class: "font-md" }, h("raul-badge", { icon: "add-circle-1", content: "88", variant: "warning" }), " ", h("code", null, "variant=\"warning\"")), h("p", { class: "font-md" }, h("raul-badge", { icon: "add-circle-1", content: "88", variant: "success" }), " ", h("code", null, "variant=\"success\"")))), h("div", { slot: "design" }, "Design Guidelines Stuff"), h("div", { slot: "api" }, h("docs-readme", { component: "raul-badge" }), h("docs-preview", { component: "raul-badge" }), h("docs-interface", { component: "raul-badge" }))));
    }
};

export { DocsRaulBadge as docs_raul_badge };
