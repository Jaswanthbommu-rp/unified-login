import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsRaulStatus = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Status');
    }
    render() {
        return (h("docs-element", { title: "Status" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", { class: "flex flex-wrap -mx-2" }, h("div", { class: "flex-none p-2" }, h("raul-status", null, "Default")), h("div", { class: "flex-none p-2" }, h("raul-status", { variant: "destructive" }, "Destructive")), h("div", { class: "flex-none p-2" }, h("raul-status", { variant: "success" }, "Success")), h("div", { class: "flex-none p-2" }, h("raul-status", { variant: "warning" }, "Warning")), h("div", { class: "w-1/2 p-2" }, h("raul-status", null, "Really long text to test the component inside of a half width container"))))), h("div", { slot: "design" }, h("docs-readme", { component: "raul-status" })), h("div", { slot: "api" }, h("docs-preview", { component: "raul-status", content: "Status" }), h("docs-interface", { component: "raul-status" }))));
    }
};

export { DocsRaulStatus as docs_raul_status };
