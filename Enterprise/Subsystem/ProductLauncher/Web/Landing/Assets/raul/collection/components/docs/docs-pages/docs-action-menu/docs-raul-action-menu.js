import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulBadge {
    componentDidLoad() {
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
    }
    render() {
        return (h("docs-element", { title: "Action Menu", ref: el => this.el = el },
            h("div", { slot: "overview" },
                h("docs-readme", { component: "raul-action-menu" }),
                h("docs-showcase", null,
                    h("raul-action-menu", { emphasizeFinal: true },
                        h("raul-action-menu-item", { url: "https://google.com" }, "Go to Google"),
                        h("raul-action-menu-item", { payload: 'test payload' }, "This is a very very very long menu item"),
                        h("raul-action-menu-item", { disabled: true }, "This is disabled"),
                        h("raul-action-menu-item", { payload: 'test-payload' }, "This is the final action")))),
            h("div", { slot: "api" },
                h("h3", { class: 'heading-lg' }, "Raul-action-menu"),
                h("docs-preview", { component: "raul-action-menu", content: `
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
          ` }),
                h("docs-interface", { component: "raul-action-menu" }),
                h("h3", { class: 'heading-lg' }, "Raul-action-menu-item"),
                h("docs-preview", { component: "raul-action-menu-item" }),
                h("docs-interface", { component: "raul-action-menu-item" })),
            h("div", { slot: "design" }, "Design Guidelines Stuff")));
    }
    static get is() { return "docs-raul-action-menu"; }
}
