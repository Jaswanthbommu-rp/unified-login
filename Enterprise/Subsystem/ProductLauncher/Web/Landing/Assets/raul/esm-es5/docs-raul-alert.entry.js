import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsRaulAlert = /** @class */ (function () {
    function DocsRaulAlert(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsRaulAlert.prototype.componentDidLoad = function () {
        initPage('Alert');
    };
    DocsRaulAlert.prototype.render = function () {
        return (h("docs-element", { title: 'Alert' }, h("div", { slot: 'overview' }, h("docs-readme", { component: 'raul-alert' }), h("docs-showcase", null, h("div", { class: 'mb-10 md:flex justify-between' }, h("div", { class: "sm:w-full md:w-1/3 my-3" }, h("raul-alert", { heading: 'Information alert', content: 'This is an information alert', ctaMessage: 'Optional', ctaUrl: 'https://www.google.com/', rounded: true })), h("div", { class: "sm:w-full md:w-1/3 md:mx-10 my-3" }, h("raul-alert", { heading: 'Information alert', content: 'This is an information alert', rounded: true })), h("div", { class: "sm:w-full md:w-1/3 my-3" }, h("raul-alert", { heading: 'Information alert', rounded: true }))), h("div", { class: 'mb-10 md:flex justify-between' }, h("div", { class: "sm:w-full md:w-1/3 my-3" }, h("raul-alert", { variant: 'success', heading: 'Success alert', content: 'This is an success alert', ctaMessage: 'Optional', ctaUrl: 'https://www.google.com/' })), h("div", { class: "sm:w-full md:w-1/3 md:mx-10 my-3" }, h("raul-alert", { variant: 'success', heading: 'Success alert', content: 'This is an success alert' })), h("div", { class: "sm:w-full md:w-1/3 my-3" }, h("raul-alert", { variant: 'success', heading: 'Success alert' }))), h("div", { class: 'mb-10 md:flex justify-between' }, h("div", { class: "sm:w-full md:w-1/3 my-3" }, h("raul-alert", { variant: 'warning', heading: 'Warning alert', content: 'This is an warning alert', ctaMessage: 'Optional', ctaUrl: 'https://www.google.com/' })), h("div", { class: "sm:w-full md:w-1/3 md:mx-10 my-3" }, h("raul-alert", { variant: 'warning', heading: 'Warning alert', content: 'This is an warning alert' })), h("div", { class: "sm:w-full md:w-1/3 my-3" }, h("raul-alert", { variant: 'warning', heading: 'Warning alert' }))), h("div", { class: 'mb-10 md:flex justify-between' }, h("div", { class: "sm:w-full md:w-1/3 my-3" }, h("raul-alert", { variant: 'danger', heading: 'Danger alert', content: 'This is an danger alert', ctaMessage: 'Optional', ctaUrl: 'https://www.google.com/', rounded: true })), h("div", { class: "sm:w-full md:w-1/3 md:mx-10 my-3" }, h("raul-alert", { variant: 'danger', heading: 'Danger alert', content: 'This is an danger alert', rounded: true })), h("div", { class: "sm:w-full md:w-1/3 my-3" }, h("raul-alert", { variant: 'danger', heading: 'Danger alert', rounded: true }))))), h("div", { slot: 'design' }), h("div", { slot: 'api' })));
    };
    return DocsRaulAlert;
}());
export { DocsRaulAlert as docs_raul_alert };
