import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulSwitch {
    componentDidLoad() {
        initPage('Switch');
    }
    render() {
        return (h("docs-element", { title: "Switch" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("raul-switch", { "label-text": "Regular switch" },
                        h("input", { type: 'checkbox', id: 'switch1', checked: true })),
                    h("raul-switch", { "label-text": "Disabled switch" },
                        h("input", { type: 'checkbox', id: 'switch2', disabled: true })),
                    h("raul-switch", { "label-text": "Small switch", small: true },
                        h("input", { type: 'checkbox', id: 'switch3', checked: true, disabled: true })))),
            h("div", { slot: "design" },
                h("docs-readme", { component: "raul-switch" })),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-switch", content: "<input type='checkbox' id='switch' checked disabled />" }),
                h("docs-interface", { component: "raul-switch" }))));
    }
    static get is() { return "docs-raul-switch"; }
}
