import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsHome = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Home', false);
    }
    render() {
        return (h("div", { class: "docs-page-container docs-home" }, h("div", { class: "docs-page-header flex flex-wrap lg:flex-row-reverse p-0 items-stretch lg:items-center min-h-screen" }, h("div", { class: "home-illustration w-full lg:w-1/2 bg-center lg:bg-left lg:min-h-screen m-8 lg:m-0 lg:p-8" }), h("div", { class: "w-full lg:w-1/2 p-8 lg:pt-0 min-h-auto lg:min-h-full" }, h("h1", null, "RAUL 3"), h("p", { class: "pb-12 pr-6" }, "The next generation of the Realpage Asset & UI Library is responsive, accessibile, and built with web components so it's also technology neutral."), h("raul-button", { variant: "primary", href: "/elements/index" }, "Components")))));
    }
    static get style() { return ".docs-home{background-color:#fff}.docs-home .docs-page-header{background:url(/assets/illustrations/bg-blob.svg) -20vw -20vw no-repeat;background-size:cover;min-height:calc(100vh - .75rem)}.docs-home .docs-page-header h1{white-space:nowrap;font-size:9vw}.docs-home .docs-page-header .home-illustration{background-image:url(/assets/illustrations/coding_pages.svg);background-repeat:no-repeat;background-size:contain;min-height:20vh}\@media (min-width:1024px){.docs-home .docs-page-header .home-illustration{min-height:100vh}}"; }
};

export { DocsHome as docs_home };
