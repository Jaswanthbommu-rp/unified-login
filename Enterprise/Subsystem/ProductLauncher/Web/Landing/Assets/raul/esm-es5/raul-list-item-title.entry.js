import { r as registerInstance, h } from './core-9263a98c.js';
var RaulListItemTitle = /** @class */ (function () {
    function RaulListItemTitle(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulListItemTitle.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulListItemTitle, "style", {
        get: function () { return "raul-list-item-title{display:block;font-size:.875rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:.25rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulListItemTitle;
}());
export { RaulListItemTitle as raul_list_item_title };
