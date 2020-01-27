import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsRaulCheckobx = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Checkbox');
    }
    render() {
        return (h("docs-element", { title: "Checkbox" }, h("div", { slot: "overview" }, h("docs-showcase", null)), h("div", { slot: "design" }, h("docs-readme", { component: "raul-checkbox" })), h("div", { slot: "api" }, h("docs-preview", { component: "raul-checkbox", content: "<input type='checkbox' name='foo' value='bar' />" }), h("docs-interface", { component: "raul-checkbox" }))));
    }
};

export { DocsRaulCheckobx as docs_raul_checkbox };
