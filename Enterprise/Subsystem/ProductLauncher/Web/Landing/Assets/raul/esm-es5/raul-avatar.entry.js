import { r as registerInstance, h } from './core-9263a98c.js';
var RaulAvatar = /** @class */ (function () {
    function RaulAvatar(hostRef) {
        registerInstance(this, hostRef);
        this.variant = 'profile';
    }
    RaulAvatar.prototype.render = function () {
        return (h("raul-icon", { icon: this.variant === 'profile' ? 'user' : 'building-7', class: "raul-avatar__icon" }));
    };
    Object.defineProperty(RaulAvatar, "style", {
        get: function () { return "raul-avatar{display:-ms-inline-flexbox;display:inline-flex;-ms-flex-align:center;align-items:center;-ms-flex-pack:center;justify-content:center;vertical-align:middle;width:2.5rem;height:2.5rem;border-radius:9999px;background-color:#ebedee}raul-avatar .raul-avatar__icon{color:#9ba3a7;font-size:1.25rem}raul-avatar[primary]{background-color:#0076cc}raul-avatar[primary] .raul-avatar__icon{color:#fff}raul-avatar[small]{width:2rem;height:2rem}raul-avatar[small] .raul-avatar__icon{font-size:1rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAvatar;
}());
export { RaulAvatar as raul_avatar };
