import { r as registerInstance, h } from './core-9263a98c.js';
var RaulCardHeaderActions = /** @class */ (function () {
    function RaulCardHeaderActions(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulCardHeaderActions.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulCardHeaderActions, "style", {
        get: function () { return "raul-card-header-actions{display:block;-ms-flex:0 1 auto;flex:0 1 auto}raul-card-header-actions a,raul-card-header-actions button{display:-ms-inline-flexbox;display:inline-flex;font-size:1.25rem;padding:.25rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulCardHeaderActions;
}());
export { RaulCardHeaderActions as raul_card_header_actions };
