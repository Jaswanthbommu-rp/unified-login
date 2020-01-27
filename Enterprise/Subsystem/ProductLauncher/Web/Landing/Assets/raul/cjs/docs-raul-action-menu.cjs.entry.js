'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulBadge = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Action Menu');
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
    }
    render() {
        return (core.h("docs-element", { title: "Action Menu", ref: el => this.el = el }, core.h("div", { slot: "overview" }, core.h("docs-readme", { component: "raul-action-menu" }), core.h("docs-showcase", null, core.h("raul-action-menu", { emphasizeFinal: true }, core.h("raul-action-menu-item", { url: "https://google.com" }, "Go to Google"), core.h("raul-action-menu-item", { payload: 'test payload' }, "This is a very very very long menu item"), core.h("raul-action-menu-item", { disabled: true }, "This is disabled"), core.h("raul-action-menu-item", { payload: 'test-payload' }, "This is the final action")))), core.h("div", { slot: "api" }, core.h("h3", { class: 'heading-lg' }, "Raul-action-menu"), core.h("docs-preview", { component: "raul-action-menu", content: `
          <raul-action-menu-item url="https://google.com">
            Go to Google
          </raul-action-menu-item>
          <raul-action-menu-item payload='test payload'>
            This is a very very very long menu item
          </raul-action-menu-item>
          <raul-action-menu-item disabled>
            This is disabled
          </raul-action-menu-item>
          <raul-action-menu-item payload='test-payload'>
            This is the final action
          </raul-action-menu-item>
          ` }), core.h("docs-interface", { component: "raul-action-menu" }), core.h("h3", { class: 'heading-lg' }, "Raul-action-menu-item"), core.h("docs-preview", { component: "raul-action-menu-item" }), core.h("docs-interface", { component: "raul-action-menu-item" })), core.h("div", { slot: "design" }, "Design Guidelines Stuff")));
    }
};

exports.docs_raul_action_menu = DocsRaulBadge;
