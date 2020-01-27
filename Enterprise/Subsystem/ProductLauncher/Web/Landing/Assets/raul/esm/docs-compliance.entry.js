import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsCompliance = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Compliance', false);
    }
    render() {
        return (h("div", { class: "docs-page-container" }, h("div", { class: "docs-page-header" }, h("docs-markdown", null, `
                # Compliance
            `)), h("div", { class: "docs-page-content" }, "Content here.")));
    }
};

export { DocsCompliance as docs_compliance };
