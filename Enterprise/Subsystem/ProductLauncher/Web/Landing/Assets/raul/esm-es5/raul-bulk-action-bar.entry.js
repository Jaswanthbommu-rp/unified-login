import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';
var RaulBulkActionBar = /** @class */ (function () {
    function RaulBulkActionBar(hostRef) {
        registerInstance(this, hostRef);
        this.buttonWidths = [];
        this.toggleOverflow = false;
        this.selectedCount = 0;
        this.totalRecords = 0;
        this.open = false;
        this.showTray = false;
        this.bulkActionsClose = createEvent(this, "bulkActionsClose", 7);
        this.bulkActionsSelectAll = createEvent(this, "bulkActionsSelectAll", 7);
    }
    RaulBulkActionBar.prototype.openChanged = function () {
        var _this = this;
        if (this.open) {
            this.getButtonWidths();
            this.handelMoreButtons(this.bulkActionEl, this.buttonWidths, this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
            setTimeout(function () {
                _this.toggleOverflow = true;
            }, 350);
        }
        else {
            this.toggleOverflow = false;
            // Reset buttons
            setTimeout(function () {
                _this.buttonWidths = [];
                _this.handelMoreButtons(_this.bulkActionEl, _this.buttonWidths, _this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
            }, 350);
        }
    };
    RaulBulkActionBar.prototype.handleResize = function () {
        if (this.buttonWidths.length > 0)
            this.handelMoreButtons(this.bulkActionEl, this.buttonWidths, this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
    };
    RaulBulkActionBar.prototype.getButtonWidths = function () {
        var _this = this;
        var buttons = this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button');
        this.buttonWidths = [];
        buttons.forEach(function (button) {
            var thisButton = button;
            _this.buttonWidths.push(thisButton.offsetWidth);
        });
    };
    RaulBulkActionBar.prototype.closeBar = function () {
        this.open = this.showTray = false;
        this.bulkActionsClose.emit();
    };
    RaulBulkActionBar.prototype.debounce = function (func, delay) {
        var inDebounce;
        return function () {
            var context = this;
            var args = arguments;
            clearTimeout(inDebounce);
            inDebounce = setTimeout(function () { return func.apply(context, args); }, delay);
        };
    };
    RaulBulkActionBar.prototype.toggleTray = function () {
        this.showTray = !this.showTray;
    };
    RaulBulkActionBar.prototype.handelMoreButtons = function (element, buttonWidths, buttons) {
        var _this = this;
        window.addEventListener('click', function (e) {
            var tray = _this.moreTray;
            // @ts-ignore
            if (tray && (!tray.contains(e.target) || e.target.classList.contains('r-button__element'))) {
                _this.showTray = false;
            }
        }, true);
        var leftSideWidth = element.querySelector('.r-bulk-actions__left').offsetWidth;
        this.buttonsInTray = [];
        var buttonsNotInTray = [];
        var visibleButtonTotalWidth = 0;
        var maxButtonWidth = Math.max.apply(Math, buttonWidths);
        buttons.forEach(function (button, index) {
            if (!button.classList.contains('r-bulk-actions__button-more')) {
                visibleButtonTotalWidth = visibleButtonTotalWidth + buttonWidths[index] + 8;
                if ((visibleButtonTotalWidth + maxButtonWidth) > leftSideWidth) {
                    _this.buttonsInTray.push(button);
                }
                else {
                    buttonsNotInTray.push(button);
                }
            }
        });
        this.buttonsInTray.forEach(function (button) {
            _this.moreTray.appendChild(button);
        });
        buttonsNotInTray.forEach(function (button) {
            element.querySelector('.r-bulk-actions__buttons-wrapper').appendChild(button);
        });
        if (this.buttonsInTray.length > 0) {
            element.querySelector('.r-bulk-actions__button-more').classList.add('r-bulk-actions__button-more--show');
        }
        else {
            element.querySelector('.r-bulk-actions__button-more').classList.remove('r-bulk-actions__button-more--show');
        }
    };
    RaulBulkActionBar.prototype.render = function () {
        var _this = this;
        return (h("div", { class: {
                'r-bulk-actions': true,
                'r-bulk-actions--expanded': this.open,
                'overflow-visible': this.toggleOverflow,
                'overflow-hidden': !this.toggleOverflow,
            }, ref: function (el) { return _this.bulkActionEl = el; } }, h("div", { class: "r-bulk-actions__wrapper" }, h("div", { class: "r-bulk-actions__left" }, h("div", { class: "r-bulk-actions__buttons-wrapper" }, h("slot", null)), h("div", { class: "r-bulk-actions__more" }, h("raul-button", { class: "r-bulk-actions__button-more", variant: "reverse", size: "small", onClick: function () { return _this.toggleTray(); } }, "More"), h("div", { class: {
                'r-bulk-actions__more-tray': true,
                'r-bulk-actions__more-tray--show': this.showTray,
            }, ref: function (el) { return _this.moreTray = el; } }))), h("div", { class: "r-bulk-actions__right" }, h("button", { type: "button", class: "r-bulk-actions__select-all", onClick: function () { return _this.bulkActionsSelectAll.emit(); } }, "Select all ", this.totalRecords, " records"), h("span", { class: "r-bulk-actions__selected-count" }, this.selectedCount, " Selected"), h("button", { type: "button", class: "r-bulk-actions__close", onClick: function () { return _this.closeBar(); } }, h("raul-icon", { icon: "close" }))))));
    };
    Object.defineProperty(RaulBulkActionBar, "watchers", {
        get: function () {
            return {
                "open": ["openChanged"]
            };
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RaulBulkActionBar, "style", {
        get: function () { return "raul-bulk-action-bar{display:block}raul-bulk-action-bar .r-bulk-actions{bottom:0;color:#fff;left:0;position:fixed;width:100%;max-height:0;-webkit-transition:max-height .35s;transition:max-height .35s;z-index:1010}raul-bulk-action-bar .r-bulk-actions--expanded{max-height:3rem}\@media (min-width:768px){raul-bulk-action-bar .r-bulk-actions{position:relative;bottom:auto}}raul-bulk-action-bar .r-bulk-actions__wrapper{-ms-flex-align:center;align-items:center;display:-ms-flexbox;display:flex;-ms-flex-wrap:nowrap;flex-wrap:nowrap;position:relative;padding:.5rem;background-color:#0076cc;height:3rem}raul-bulk-action-bar .r-bulk-actions__left{display:-ms-flexbox;display:flex;margin-right:auto;-ms-flex-positive:1;flex-grow:1}raul-bulk-action-bar .r-bulk-actions__buttons-wrapper{display:inline-block}raul-bulk-action-bar .r-bulk-actions__buttons-wrapper .r-button{margin-right:.5rem}raul-bulk-action-bar .r-bulk-actions__more{display:inline-block;position:relative}raul-bulk-action-bar .r-bulk-actions__button-more{visibility:hidden}raul-bulk-action-bar .r-bulk-actions__button-more--show{visibility:visible}raul-bulk-action-bar .r-bulk-actions__right{-ms-flex-align:center;align-items:center;display:-ms-flexbox;display:flex;font-size:.75rem;-ms-flex-pack:end;justify-content:flex-end;-ms-flex:0 0 auto;flex:0 0 auto;max-width:265px;min-width:55px}raul-bulk-action-bar .r-bulk-actions__selected-count{font-size:.75rem}raul-bulk-action-bar .r-bulk-actions__select-all{padding-top:0;padding-bottom:0;padding-left:.75rem;padding-right:.75rem;display:none}raul-bulk-action-bar .r-bulk-actions__select-all:hover{text-decoration:underline}raul-bulk-action-bar .r-bulk-actions__select-all:after{display:inline-block;margin-left:.5rem;content:\"|\"}\@media only screen and (min-width:768px){raul-bulk-action-bar .r-bulk-actions__select-all{display:inline-block}}raul-bulk-action-bar .r-bulk-actions__close{padding:.75rem}raul-bulk-action-bar .r-bulk-actions__close raul-icon{font-size:.875rem}raul-bulk-action-bar .r-bulk-actions__more-tray{left:0;overflow:hidden;padding-top:.5rem;padding-left:.5rem;padding-right:0;padding-bottom:0;position:fixed;width:100%;background:#37474f;bottom:-100%;min-height:48px;-webkit-transition:bottom .35s;transition:bottom .35s;z-index:-1}raul-bulk-action-bar .r-bulk-actions__more-tray--show{bottom:48px}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button{margin-bottom:.5rem;margin-right:.5rem}\@media (min-width:768px){raul-bulk-action-bar .r-bulk-actions__more-tray{background-color:#fff;border-radius:.125rem;bottom:auto;display:none;padding-top:.5rem;padding-bottom:.5rem;padding-left:0;padding-right:0;position:absolute;right:auto;vertical-align:middle;-webkit-box-shadow:0 8px 16px 0 rgba(82,97,115,.18);box-shadow:0 8px 16px 0 rgba(82,97,115,.18);top:36px;width:180px;z-index:2000}raul-bulk-action-bar .r-bulk-actions__more-tray--show{display:block;bottom:auto}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button{display:block;margin-right:0}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button .r-button--reverse .r-button__content{-ms-flex-align:center;align-items:center;background-color:transparent;display:-ms-flexbox;display:flex;font-size:.75rem;font-weight:400;-ms-flex-pack:start;justify-content:flex-start;padding-top:0;padding-bottom:0;padding-left:1rem;padding-right:1rem;color:#37474f;min-height:32px}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button .r-button--reverse .r-button__focus-ring *{display:none}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button .r-button--reverse .r-button__element:hover~.r-button__content{background-color:transparent;text-decoration:underline}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button .r-button--reverse .r-button__content:before{border-color:#0076cc!important;border-radius:0!important;top:2px!important;right:2px!important;bottom:2px!important;left:2px!important}}"; },
        enumerable: true,
        configurable: true
    });
    return RaulBulkActionBar;
}());
export { RaulBulkActionBar as raul_bulk_action_bar };
