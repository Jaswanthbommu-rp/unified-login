'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulBadge = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Filter Menu');
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
    }
    render() {
        return (core.h("docs-element", { title: "Filter Menu" }, core.h("div", { slot: "overview" }, core.h("docs-readme", { component: "raul-filter-menu" }), core.h("docs-showcase", null, core.h("raul-filter-menu", null, core.h("raul-filter-menu-item", { icon: 'alien-head' }, "First item"), core.h("raul-filter-menu-item", { icon: 'artist' }, "Second item"), core.h("raul-filter-menu-item", { icon: 'astronaut' }, "A very very very long item")))), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, core.h("h3", { class: 'heading-lg' }, "Raul-filter-menu"), core.h("docs-preview", { component: "raul-filter-menu", content: `
          <raul-filter-menu-item icon='alien-head'>
            First item
          </raul-filter-menu-item>
          <raul-filter-menu-item icon='artist'>
            Second item
          </raul-filter-menu-item>
          <raul-filter-menu-item icon='astronaut'>
            A very very very long item
          </raul-filter-menu-item>
          ` }), core.h("docs-interface", { component: "raul-filter-menu" }), core.h("h3", { class: 'heading-lg' }, "Raul-filter-menu-item"), core.h("docs-preview", { component: "raul-filter-menu-item" }), core.h("docs-interface", { component: "raul-filter-menu-item" }))));
    }
};

exports.docs_raul_filter_menu = DocsRaulBadge;
