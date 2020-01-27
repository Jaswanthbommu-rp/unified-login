import { r as registerInstance, h } from './core-9263a98c.js';
var RaulListItemSubtitle = /** @class */ (function () {
    function RaulListItemSubtitle(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulListItemSubtitle.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulListItemSubtitle, "style", {
        get: function () { return "raul-list-item-subtitle{display:block}"; },
        enumerable: true,
        configurable: true
    });
    return RaulListItemSubtitle;
}());
export { RaulListItemSubtitle as raul_list_item_subtitle };
