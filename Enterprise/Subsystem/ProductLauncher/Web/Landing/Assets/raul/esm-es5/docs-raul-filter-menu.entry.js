import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsRaulBadge = /** @class */ (function () {
    function DocsRaulBadge(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsRaulBadge.prototype.componentDidLoad = function () {
        initPage('Filter Menu');
        // document.querySelector('raul-filter-menu').items = [
        //   {
        //     title: 'Lorem ipsum',
        //     payload: 'payload-1',
        //     icon: 'alien-head'
        //   },
        //   {
        //     title: 'Dolor sit amet',
        //     payload: 'payload-2',
        //     icon: 'astronaut'
        //   },
        //   {
        //     title: 'Consectetur adipiscing elit',
        //     payload: 'payload-3',
        //     icon: 'artist'
        //   }
        // ]
    };
    DocsRaulBadge.prototype.render = function () {
        return (h("docs-element", { title: "Filter Menu" }, h("div", { slot: "overview" }, h("docs-readme", { component: "raul-filter-menu" }), h("docs-showcase", null, h("raul-filter-menu", null, h("raul-filter-menu-item", { icon: 'alien-head' }, "First item"), h("raul-filter-menu-item", { icon: 'artist' }, "Second item"), h("raul-filter-menu-item", { icon: 'astronaut' }, "A very very very long item")))), h("div", { slot: "design" }, "Design Guidelines Stuff"), h("div", { slot: "api" }, h("h3", { class: 'heading-lg' }, "Raul-filter-menu"), h("docs-preview", { component: "raul-filter-menu", content: "\n          <raul-filter-menu-item icon='alien-head'>\n            First item\n          </raul-filter-menu-item>\n          <raul-filter-menu-item icon='artist'>\n            Second item\n          </raul-filter-menu-item>\n          <raul-filter-menu-item icon='astronaut'>\n            A very very very long item\n          </raul-filter-menu-item>\n          " }), h("docs-interface", { component: "raul-filter-menu" }), h("h3", { class: 'heading-lg' }, "Raul-filter-menu-item"), h("docs-preview", { component: "raul-filter-menu-item" }), h("docs-interface", { component: "raul-filter-menu-item" }))));
    };
    return DocsRaulBadge;
}());
export { DocsRaulBadge as docs_raul_filter_menu };
