import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulBadge {
    componentDidLoad() {
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
    }
    render() {
        return (h("docs-element", { title: "Filter Menu" },
            h("div", { slot: "overview" },
                h("docs-readme", { component: "raul-filter-menu" }),
                h("docs-showcase", null,
                    h("raul-filter-menu", null,
                        h("raul-filter-menu-item", { icon: 'alien-head' }, "First item"),
                        h("raul-filter-menu-item", { icon: 'artist' }, "Second item"),
                        h("raul-filter-menu-item", { icon: 'astronaut' }, "A very very very long item")))),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" },
                h("h3", { class: 'heading-lg' }, "Raul-filter-menu"),
                h("docs-preview", { component: "raul-filter-menu", content: `
          <raul-filter-menu-item icon='alien-head'>
            First item
          </raul-filter-menu-item>
          <raul-filter-menu-item icon='artist'>
            Second item
          </raul-filter-menu-item>
          <raul-filter-menu-item icon='astronaut'>
            A very very very long item
          </raul-filter-menu-item>
          ` }),
                h("docs-interface", { component: "raul-filter-menu" }),
                h("h3", { class: 'heading-lg' }, "Raul-filter-menu-item"),
                h("docs-preview", { component: "raul-filter-menu-item" }),
                h("docs-interface", { component: "raul-filter-menu-item" }))));
    }
    static get is() { return "docs-raul-filter-menu"; }
}
