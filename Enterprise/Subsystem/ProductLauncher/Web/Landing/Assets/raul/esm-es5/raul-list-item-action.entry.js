import { r as registerInstance, h } from './core-9263a98c.js';
var RaulListItemAction = /** @class */ (function () {
    function RaulListItemAction(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulListItemAction.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulListItemAction, "style", {
        get: function () { return "raul-list-item-action,raul-list-item-action .r-list__item__action{display:-ms-inline-flexbox;display:inline-flex}raul-list-item-action .r-list__item__action{font-size:.875rem;color:#0076cc;padding:.5rem;margin:-.5rem}raul-list-item-action .r-list__item__action raul-icon{font-size:1rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulListItemAction;
}());
export { RaulListItemAction as raul_list_item_action };
