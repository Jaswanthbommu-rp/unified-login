import { r as registerInstance, h, H as Host } from './core-9263a98c.js';
var RaulListItem = /** @class */ (function () {
    function RaulListItem(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulListItem.prototype.render = function () {
        return (h(Host, { role: "listitem" }, h("slot", null)));
    };
    Object.defineProperty(RaulListItem, "style", {
        get: function () { return "raul-list-item{display:block;border-bottom-width:1px;border-color:#ebedee;padding-top:1rem;padding-bottom:1rem;font-size:.875rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulListItem;
}());
export { RaulListItem as raul_list_item };
