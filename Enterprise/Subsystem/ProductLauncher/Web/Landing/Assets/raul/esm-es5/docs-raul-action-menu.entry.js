import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsRaulBadge = /** @class */ (function () {
    function DocsRaulBadge(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsRaulBadge.prototype.componentDidLoad = function () {
        initPage('Action Menu');
        // document.querySelector('raul-action-menu').items = [
        //   {
        //     title: 'Lorem ipsum',
        //     payload: 'payload-1'
        //   },
        //   {
        //     title: 'Dolor sit amet',
        //     payload: 'payload-2'
        //   },
        //   {
        //     title: 'This title is very very very very long',
        //     payload: 'payload-3'
        //   },
        //   {
        //     title: 'Go to google',
        //     payload: 'payload-3',
        //     url: 'https://google.com'
        //   }
        // ]
        // https://github.realpage.com/jeff-lloyd/raul-3.git
    };
    DocsRaulBadge.prototype.render = function () {
        var _this = this;
        return (h("docs-element", { title: "Action Menu", ref: function (el) { return _this.el = el; } }, h("div", { slot: "overview" }, h("docs-readme", { component: "raul-action-menu" }), h("docs-showcase", null, h("raul-action-menu", { emphasizeFinal: true }, h("raul-action-menu-item", { url: "https://google.com" }, "Go to Google"), h("raul-action-menu-item", { payload: 'test payload' }, "This is a very very very long menu item"), h("raul-action-menu-item", { disabled: true }, "This is disabled"), h("raul-action-menu-item", { payload: 'test-payload' }, "This is the final action")))), h("div", { slot: "api" }, h("h3", { class: 'heading-lg' }, "Raul-action-menu"), h("docs-preview", { component: "raul-action-menu", content: "\n          <raul-action-menu-item url=\"https://google.com\">\n            Go to Google\n          </raul-action-menu-item>\n          <raul-action-menu-item payload='test payload'>\n            This is a very very very long menu item\n          </raul-action-menu-item>\n          <raul-action-menu-item disabled>\n            This is disabled\n          </raul-action-menu-item>\n          <raul-action-menu-item payload='test-payload'>\n            This is the final action\n          </raul-action-menu-item>\n          " }), h("docs-interface", { component: "raul-action-menu" }), h("h3", { class: 'heading-lg' }, "Raul-action-menu-item"), h("docs-preview", { component: "raul-action-menu-item" }), h("docs-interface", { component: "raul-action-menu-item" })), h("div", { slot: "design" }, "Design Guidelines Stuff")));
    };
    return DocsRaulBadge;
}());
export { DocsRaulBadge as docs_raul_action_menu };
