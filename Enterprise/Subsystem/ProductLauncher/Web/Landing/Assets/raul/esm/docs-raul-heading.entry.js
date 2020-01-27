import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsRaulheading = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('heading');
    }
    render() {
        return (h("docs-element", { title: "Heading" }, h("div", { slot: "overview" }, h("docs-readme", { component: "raul-heading" })), h("div", { slot: "design" }, "Design Guidelines Stuff"), h("div", { slot: "api" }, h("docs-preview", { component: "raul-heading", content: "Lorem ipsum dolor sit amet" }), h("docs-showcase", null), h("docs-interface", { component: "raul-heading" }))));
    }
};

export { DocsRaulheading as docs_raul_heading };
