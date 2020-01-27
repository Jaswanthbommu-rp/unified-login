import { r as registerInstance, t as toast, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsRaulSnackbar = /** @class */ (function () {
    function DocsRaulSnackbar(hostRef) {
        var _this = this;
        registerInstance(this, hostRef);
        this.counter = 0;
        this.handleClick = function () {
            _this.counter++;
            toast({
                timeout: 3000,
                heading: "Snackbar #" + _this.counter,
                content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
                ctaMessage: 'Click me!',
                tagName: 'raul-snackbar',
                dismissable: true
            });
        };
    }
    DocsRaulSnackbar.prototype.componentDidLoad = function () {
        initPage('Snackbar');
    };
    DocsRaulSnackbar.prototype.render = function () {
        return (h("docs-element", { title: 'Snackbar' }, h("div", { slot: 'overview' }, h("docs-readme", { component: 'raul-snackbar' }), h("docs-showcase", null, h("div", { class: 'mb-10' }, h("raul-snackbar", { dismissable: true, variant: 'information', heading: 'This is an information snackbar' })), h("div", { class: 'mb-10' }, h("raul-snackbar", { dismissable: true, variant: 'success', heading: 'This is a success snackbar' })), h("div", { class: 'mb-10' }, h("raul-snackbar", { dismissable: true, variant: 'warning', heading: 'This a warning snackbar' })), h("div", { class: 'mb-10' }, h("raul-snackbar", { dismissable: true, variant: 'danger', heading: 'This is a danger snackbar' })), h("div", { class: 'mb-10' }, h("raul-snackbar", { dismissable: true, variant: 'information', heading: 'This is a complex snackbar', content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed dictum dui ut ante mollis, in viverra ipsum aliquam.', ctaMessage: 'Go to Google', ctaUrl: 'https://google.com' })), h("raul-button", { onClick: this.handleClick }, "Show snackbar"))), h("div", { slot: 'design' }), h("div", { slot: 'api' }, h("docs-preview", { component: "raul-snackbar" }), h("docs-interface", { component: "raul-snackbar" }))));
    };
    return DocsRaulSnackbar;
}());
export { DocsRaulSnackbar as docs_raul_snackbar };
