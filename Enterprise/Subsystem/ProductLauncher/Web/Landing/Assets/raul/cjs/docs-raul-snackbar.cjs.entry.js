'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulSnackbar = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.counter = 0;
        this.handleClick = () => {
            this.counter++;
            core.toast({
                timeout: 3000,
                heading: `Snackbar #${this.counter}`,
                content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
                ctaMessage: 'Click me!',
                tagName: 'raul-snackbar',
                dismissable: true
            });
        };
    }
    componentDidLoad() {
        initPage.initPage('Snackbar');
    }
    render() {
        return (core.h("docs-element", { title: 'Snackbar' }, core.h("div", { slot: 'overview' }, core.h("docs-readme", { component: 'raul-snackbar' }), core.h("docs-showcase", null, core.h("div", { class: 'mb-10' }, core.h("raul-snackbar", { dismissable: true, variant: 'information', heading: 'This is an information snackbar' })), core.h("div", { class: 'mb-10' }, core.h("raul-snackbar", { dismissable: true, variant: 'success', heading: 'This is a success snackbar' })), core.h("div", { class: 'mb-10' }, core.h("raul-snackbar", { dismissable: true, variant: 'warning', heading: 'This a warning snackbar' })), core.h("div", { class: 'mb-10' }, core.h("raul-snackbar", { dismissable: true, variant: 'danger', heading: 'This is a danger snackbar' })), core.h("div", { class: 'mb-10' }, core.h("raul-snackbar", { dismissable: true, variant: 'information', heading: 'This is a complex snackbar', content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed dictum dui ut ante mollis, in viverra ipsum aliquam.', ctaMessage: 'Go to Google', ctaUrl: 'https://google.com' })), core.h("raul-button", { onClick: this.handleClick }, "Show snackbar"))), core.h("div", { slot: 'design' }), core.h("div", { slot: 'api' }, core.h("docs-preview", { component: "raul-snackbar" }), core.h("docs-interface", { component: "raul-snackbar" }))));
    }
};

exports.docs_raul_snackbar = DocsRaulSnackbar;
