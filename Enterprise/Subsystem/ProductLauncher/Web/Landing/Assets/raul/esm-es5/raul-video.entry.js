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
import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';
var RaulVideo = /** @class */ (function () {
    function class_1(hostRef) {
        registerInstance(this, hostRef);
        this.src = null;
        this.autoplay = false;
        this.controls = true;
        this.videoPlay = createEvent(this, "videoPlay", 7);
        this.videoPause = createEvent(this, "videoPause", 7);
        this.videoEnded = createEvent(this, "videoEnded", 7);
    }
    class_1.prototype.componentDidLoad = function () {
        var _this = this;
        this.videoPlayer.addEventListener('play', function () { return _this.videoPlay.emit(); });
        this.videoPlayer.addEventListener('pause', function () { return _this.videoPause.emit(); });
        this.videoPlayer.addEventListener('ended', function () { return _this.videoEnded.emit(); });
    };
    class_1.prototype.playVideo = function () {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            return __generator(this, function (_a) {
                this.videoPlayer.play().then(function () {
                    // Great!
                }).catch(function () {
                    // Maybe video is not loaded yet
                    // Start playing when it is
                    _this.videoPlayer.autoplay = true;
                });
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.pauseVideo = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.videoPlayer.pause();
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.stopVideo = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.videoPlayer.pause();
                this.videoPlayer.currentTime = 0;
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.progress = function () {
        return __awaiter(this, void 0, void 0, function () {
            var _a, currentTime, duration;
            return __generator(this, function (_b) {
                _a = this.videoPlayer, currentTime = _a.currentTime, duration = _a.duration;
                return [2 /*return*/, Math.ceil(currentTime / duration * 100.00)];
            });
        });
    };
    class_1.prototype.render = function () {
        var _this = this;
        return (h("div", { class: "r-raul-video" }, h("video", { src: this.src, controls: this.controls, autoplay: this.autoplay, ref: function (el) { return _this.videoPlayer = el; } })));
    };
    Object.defineProperty(class_1, "style", {
        get: function () { return ".r-raul-video{display:block}.r-raul-video video{display:block;width:100%}.r-raul-video video::-webkit-media-controls-panel{background-image:-webkit-gradient(linear,left top,left bottom,from(transparent),to(transparent))!important;background-image:linear-gradient(transparent,transparent)!important;-webkit-filter:brightness(.4);filter:brightness(.4)}"; },
        enumerable: true,
        configurable: true
    });
    return class_1;
}());
export { RaulVideo as raul_video };
