var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
import { r as registerInstance, c as createEvent, h, g as getElement } from './core-9263a98c.js';
import { J as JavascriptTimeAgo, e as en } from './index-58cb3f49.js';
import './_commonjsHelpers-f34b4464.js';
// Polyfill for javascript-time-ago / IE11
if (!Math.sign) {
    Math.sign = function (n) {
        return ((n > 0) - (n < 0)) || +n;
    };
}
JavascriptTimeAgo.addLocale(en);
var timeAgo = new JavascriptTimeAgo('en-US');
var RaulToast = /** @class */ (function () {
    function class_1(hostRef) {
        registerInstance(this, hostRef);
        this.severity = null;
        this.actions = null;
        this.refreshKey = 0;
        this.hidden = false;
        this.timedOut = createEvent(this, "timedOut", 7);
        this.toastAction = createEvent(this, "toastAction", 7);
    }
    class_1.prototype.componentWillLoad = function () {
        var _this = this;
        // Initialize self-destruct mechanism
        if (this.timeout) {
            this.timeoutTimer = window.setTimeout(function () {
                _this.timedOut.emit();
                _this.dismiss();
            }, this.timeout);
            this.timeoutStartedAt = new Date().getTime();
        }
        // Save timestamp at moment of creation
        this.createdAt = new Date();
        // Re-render component every 1 minute (to update timestamp)
        this.refreshTimer = window.setInterval(function () {
            _this.refreshKey = _this.refreshKey + 1;
        }, 60000);
    };
    class_1.prototype.disconnectedCallback = function () {
        clearTimeout(this.timeoutTimer);
        clearInterval(this.refreshTimer);
    };
    class_1.prototype.handleMouseenter = function () {
        if (this.timeout) {
            var timeout = this.timeoutLeft ? this.timeoutLeft : this.timeout;
            var timeoutPausedAt = new Date().getTime();
            this.timeoutLeft = timeout - (timeoutPausedAt - this.timeoutStartedAt);
            clearTimeout(this.timeoutTimer);
        }
    };
    class_1.prototype.handleMouseleave = function () {
        var _this = this;
        if (this.timeout) {
            this.timeoutTimer = window.setTimeout(function () {
                _this.timedOut.emit();
                _this.dismiss();
            }, this.timeoutLeft);
            this.timeoutStartedAt = new Date().getTime();
        }
    };
    class_1.prototype.dismiss = function () {
        return __awaiter(this, void 0, void 0, function () {
            var animationDuration;
            var _this = this;
            return __generator(this, function (_a) {
                this.hidden = true;
                animationDuration = parseFloat(window.getComputedStyle(this.toastEl).animationDuration) * 1000;
                setTimeout(function () { return _this.el.remove(); }, animationDuration);
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.createdAtTimeAgo = function () {
        return timeAgo.format(this.createdAt);
    };
    class_1.prototype.emitToastAction = function (e, label) {
        e.stopPropagation();
        this.toastAction.emit(label);
    };
    class_1.prototype.render = function () {
        var _this = this;
        return (h("div", { class: {
                'r-toast': true,
                'r-toast--has-avatar': !!this.avatar,
                'r-toast--hidden': this.hidden
            }, ref: function (el) { return _this.toastEl = el; } }, h("div", { class: {
                'r-toast__header': true,
                'r-toast__header--read': this.read,
            } }, h("div", { class: "r-toast__origin" }, this.origin), h("div", { class: "r-toast__timestamp" }, this.createdAtTimeAgo()), h("button", { type: "button", class: "r-toast__dismiss", onClick: function (e) { return _this.emitToastAction(e, 'dismiss'); } }, h("raul-icon", { icon: "arrow-right-1" }))), h("div", { class: "r-toast__body" }, this.avatar &&
            h("div", { class: "r-toast__avatar" }, h("img", { src: this.avatar })), this.read &&
            h("div", { class: "r-toast__status-wrapper" }, h("div", { class: {
                    'r-toast__status': true,
                    'r-toast__status--unread': true,
                } })), h("div", { class: "r-toast__content" }, h("div", { class: "r-toast__title" }, this.heading), h("div", { class: {
                'r-toast__text': true,
                'truncate': this.read
            } }, this.body), h("div", { class: "r-toast__meta" }, this.meta), this.severity && this.severity === "High" &&
            h("div", { class: "r-toast__priority" }, this.severity))), h("div", { class: "r-toast__footer" }, this.actions && this.actions.map(function (action) { return h("button", { type: "button", class: "r-toast__action", onClick: function (e) { return _this.emitToastAction(e, action.label); } }, action.text); }))));
    };
    Object.defineProperty(class_1.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(class_1, "style", {
        get: function () { return "raul-toast{display:block}raul-toast .r-toast{background-color:#fff;width:22.5rem;padding-left:.75rem;padding-right:.75rem;padding-top:1rem;padding-bottom:1rem;margin-top:1rem;-webkit-box-shadow:0 8px 16px 0 rgba(82,97,115,.18);box-shadow:0 8px 16px 0 rgba(82,97,115,.18);position:relative;overflow:hidden;opacity:1;-webkit-animation:slideIn 1s;animation:slideIn 1s}raul-toast .r-toast__header{display:-ms-flexbox;display:flex;-ms-flex-pack:justify;justify-content:space-between;margin-bottom:.25rem;font-size:.75rem;position:relative}raul-toast .r-toast__header--read{padding-left:1rem}raul-toast .r-toast__origin,raul-toast .r-toast__timestamp{font-size:.75rem}raul-toast .r-toast__timestamp{color:#9ba3a7;-webkit-transition:opacity .35s linear;transition:opacity .35s linear}raul-toast .r-toast__timestamp:first-letter{text-transform:capitalize}raul-toast .r-toast__body{display:-ms-flexbox;display:flex;margin-top:.25rem}raul-toast .r-toast__avatar{width:3rem;height:3rem;margin-right:1rem}raul-toast .r-toast__avatar img{width:100%;height:auto}raul-toast .r-toast__dismiss{color:#c6ccd0;font-size:1.375rem;line-height:1;display:block;position:absolute;right:0;top:-4px;opacity:0;cursor:pointer;-webkit-transition:opacity .35s linear;transition:opacity .35s linear}raul-toast .r-toast__content{-ms-flex:1 1 0%;flex:1 1 0%;min-width:0}raul-toast .r-toast__title{font-size:.875rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}raul-toast .r-toast__priority{font-size:.75rem;display:inline-block;margin-top:.25rem;font-weight:500;padding:3px 6px;color:#d01a1f;background-color:#fae8e9}raul-toast .r-toast__meta{font-size:.75rem;color:#9ba3a7;font-style:italic}raul-toast .r-toast__footer{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;-ms-flex-pack:end;justify-content:flex-end}raul-toast .r-toast__action{color:#0076cc;text-align:right;font-weight:500;text-transform:uppercase;margin-top:.25rem;margin-left:.75rem}raul-toast .r-toast__action:hover{text-decoration:underline}raul-toast .r-toast__status-wrapper{padding-top:.25rem;padding-bottom:.25rem;margin-right:.5rem}raul-toast .r-toast__status{width:9px;height:9px;border-radius:50%}raul-toast .r-toast__status--unread{background:#f65216}raul-toast .r-toast--has-avatar .r-toast__header{margin-left:4rem}raul-toast .r-toast--hidden{-webkit-animation:slideOut 1s;animation:slideOut 1s}\@media (max-width:640px){raul-toast .r-toast{width:100%}}raul-toast .r-toast:hover .r-toast__timestamp{opacity:0}raul-toast .r-toast:hover .r-toast__dismiss{opacity:1}\@-webkit-keyframes slideIn{0%{opacity:0;-webkit-transform:translate(100%,-10px);transform:translate(100%,-10px)}to{opacity:1;-webkit-transform:translate(0);transform:translate(0)}}\@keyframes slideIn{0%{opacity:0;-webkit-transform:translate(100%,-10px);transform:translate(100%,-10px)}to{opacity:1;-webkit-transform:translate(0);transform:translate(0)}}\@-webkit-keyframes slideOut{0%{opacity:1;-webkit-transform:translate(0);transform:translate(0)}to{opacity:0;-webkit-transform:translate(100%,-10px);transform:translate(100%,-10px)}}\@keyframes slideOut{0%{opacity:1;-webkit-transform:translate(0);transform:translate(0)}to{opacity:0;-webkit-transform:translate(100%,-10px);transform:translate(100%,-10px)}}"; },
        enumerable: true,
        configurable: true
    });
    return class_1;
}());
export { RaulToast as raul_toast };
