import { r as registerInstance, h } from './core-9263a98c.js';
var RaulTabPane = /** @class */ (function () {
    function RaulTabPane(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulTabPane.prototype.render = function () {
        return (h("div", { class: "tabs", role: "tabpanel", id: this.name, "aria-labelledby": this.name + "-tab" }, h("slot", null)));
    };
    return RaulTabPane;
}());
export { RaulTabPane as raul_tab_pane };
