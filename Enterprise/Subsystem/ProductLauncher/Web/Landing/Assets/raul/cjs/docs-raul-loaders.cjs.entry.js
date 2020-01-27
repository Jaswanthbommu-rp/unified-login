'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulLoaders = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.showPageLoader = false;
        this.showContainerLoader = false;
    }
    componentDidLoad() {
        initPage.initPage('Loaders');
        window.addEventListener('keydown', e => {
            if (['Esc', 'Escape'].includes(e.key))
                this.showPageLoader = false;
        });
    }
    render() {
        return (core.h("docs-element", { title: "Loaders" }, core.h("div", { slot: "overview" }, core.h("raul-heading", { variant: 'page' }, "Loaders"), core.h("raul-heading", { variant: 'section' }, "Page loader"), core.h("raul-heading", { variant: 'content' }, "Usage"), core.h("raul-content", null, core.h("p", null, "You render a ", core.h("code", null, `<raul-page-loader></raul-page-loader>`), " wherever and whenever you need a page loader.")), core.h("raul-heading", { variant: 'content' }, "Examples"), core.h("raul-button", { variant: 'secondary', size: 'small', onClick: () => this.showPageLoader = true, class: "mb-12" }, "Show page loader"), core.h("raul-content", null, core.h("p", null, "*Press Escape to hide the page loader")), this.showPageLoader &&
            core.h("raul-page-loader", null), core.h("raul-heading", { variant: 'section' }, "Container loader"), core.h("raul-heading", { variant: 'content' }, "Usage"), core.h("raul-content", null, core.h("p", null, "You render a ", core.h("code", null, `<raul-container-loader></raul-container-loader>`), " as a child of the container that needs to be covered by the loader. The container needs to have ", core.h("code", null, "position: relative"))), core.h("raul-button", { variant: 'secondary', size: 'small', onClick: () => this.showContainerLoader = !this.showContainerLoader, class: "mb-8" }, this.showContainerLoader ? 'Hide' : 'Show', " container loader"), core.h("div", { class: "relative mb-12 p-6 w-full border border-gray-light" }, this.showContainerLoader && core.h("raul-container-loader", null), core.h("raul-content", null, core.h("p", null, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas molestie bibendum felis at pulvinar. Duis porttitor nulla ligula, in volutpat ex iaculis nec. Mauris sollicitudin auctor turpis, a vehicula diam accumsan ultrices. Aenean lobortis dolor elit, id sodales felis volutpat sit amet. Donec elementum malesuada lorem non tincidunt. Morbi purus lectus, pharetra ac consequat vitae, interdum placerat tortor. Phasellus ac auctor dolor. Proin cursus, elit dignissim placerat porta, dolor metus tristique nunc, at fringilla mauris purus sed dolor. Suspendisse imperdiet leo ut nulla rutrum tristique nec in orci. Proin sit amet mauris fermentum, euismod metus nec, convallis dolor. Nulla et ornare nisi. Nunc at ligula rhoncus, venenatis purus vel, sodales dolor. Fusce finibus augue sem, ut volutpat arcu fermentum at.")), core.h("docs-interface", { component: "raul-loaders" }))), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, "API Stuff")));
    }
};

exports.docs_raul_loaders = DocsRaulLoaders;
