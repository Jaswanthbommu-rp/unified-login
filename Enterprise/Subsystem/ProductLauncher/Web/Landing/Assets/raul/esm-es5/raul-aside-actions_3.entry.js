import { r as registerInstance, h } from './core-9263a98c.js';
var RaulAsideActions = /** @class */ (function () {
    function RaulAsideActions(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAsideActions.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulAsideActions, "style", {
        get: function () { return "raul-aside-actions{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;-ms-flex-pack:end;justify-content:flex-end}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAsideActions;
}());
var RaulAsideSubtitle = /** @class */ (function () {
    function RaulAsideSubtitle(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAsideSubtitle.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulAsideSubtitle, "style", {
        get: function () { return "raul-aside-subtitle{display:block;font-size:.875rem;line-height:1.25;margin-bottom:.5rem;color:rgba(55,71,79,.8);margin-bottom:0;margin-top:.25rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAsideSubtitle;
}());
var RaulAsideTitle = /** @class */ (function () {
    function RaulAsideTitle(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAsideTitle.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulAsideTitle, "style", {
        get: function () { return "raul-aside-title{display:block;font-size:1.25rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAsideTitle;
}());
export { RaulAsideActions as raul_aside_actions, RaulAsideSubtitle as raul_aside_subtitle, RaulAsideTitle as raul_aside_title };
