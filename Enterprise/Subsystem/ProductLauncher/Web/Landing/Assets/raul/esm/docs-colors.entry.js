import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsColors = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Colors', false);
    }
    render() {
        return (h("docs-element", { title: "Colors" }, h("div", { slot: "overview" }, "Overview"), h("div", { slot: "design" }, "Design Guidelines Stuff"), h("div", { slot: "api" }, "API Stuff")));
    }
};

export { DocsColors as docs_colors };
