import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsUpgrading = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Upgrading', false);
    }
    render() {
        return (h("div", { class: "docs-page-container" }, h("div", { class: "docs-page-header" }, h("docs-markdown", null, `
                # Upgrading
            `)), h("div", { class: "docs-page-content" }, "Stuff.")));
    }
};

export { DocsUpgrading as docs_upgrading };
