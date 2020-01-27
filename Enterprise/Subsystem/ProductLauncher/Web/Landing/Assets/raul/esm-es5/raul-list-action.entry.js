import { r as registerInstance, h } from './core-9263a98c.js';
var RaulListAction = /** @class */ (function () {
    function RaulListAction(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulListAction.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulListAction, "style", {
        get: function () { return "raul-list-action,raul-list-action .r-list__action{display:-ms-inline-flexbox;display:inline-flex}raul-list-action .r-list__action{font-size:.875rem;color:#0076cc;padding:.5rem;margin:-.5rem}raul-list-action .r-list__action raul-icon{font-size:1rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulListAction;
}());
export { RaulListAction as raul_list_action };
