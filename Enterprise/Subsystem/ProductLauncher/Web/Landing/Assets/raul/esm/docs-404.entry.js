import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsNotFound = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Page not found', false);
    }
    render() {
        return (h("div", { class: "docs-page-container" }, h("div", { class: "docs-page-header" }, h("docs-markdown", null, `
                # 404
                These aren't the droids your looking for
            `))));
    }
    static get style() { return ""; }
};

export { DocsNotFound as docs_404 };
