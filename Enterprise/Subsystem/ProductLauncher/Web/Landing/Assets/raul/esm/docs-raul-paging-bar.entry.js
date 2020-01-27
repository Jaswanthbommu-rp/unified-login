import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsRaulPagingBar = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Paging Bar');
    }
    render() {
        return (h("docs-element", { title: "Paging Bar" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("raul-paging-bar", { "total-rows": "55" }))), h("div", { slot: "design" }, h("docs-readme", { component: "raul-paging-bar" })), h("div", { slot: "api" }, h("docs-preview", { component: "raul-paging-bar", content: "Paging Bar" }), h("docs-interface", { component: "raul-paging-bar" }))));
    }
};

export { DocsRaulPagingBar as docs_raul_paging_bar };
