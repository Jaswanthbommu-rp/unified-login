import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';

const RaulBulkActionBar = class {
    constructor(hostRef) {
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
    openChanged() {
        if (this.open) {
            this.getButtonWidths();
            this.handelMoreButtons(this.bulkActionEl, this.buttonWidths, this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
            setTimeout(() => {
                this.toggleOverflow = true;
            }, 350);
        }
        else {
            this.toggleOverflow = false;
            // Reset buttons
            setTimeout(() => {
                this.buttonWidths = [];
                this.handelMoreButtons(this.bulkActionEl, this.buttonWidths, this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
            }, 350);
        }
    }
    handleResize() {
        if (this.buttonWidths.length > 0)
            this.handelMoreButtons(this.bulkActionEl, this.buttonWidths, this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
    }
    getButtonWidths() {
        let buttons = this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button');
        this.buttonWidths = [];
        buttons.forEach((button) => {
            let thisButton = button;
            this.buttonWidths.push(thisButton.offsetWidth);
        });
    }
    closeBar() {
        this.open = this.showTray = false;
        this.bulkActionsClose.emit();
    }
    debounce(func, delay) {
        let inDebounce;
        return function () {
            const context = this;
            const args = arguments;
            clearTimeout(inDebounce);
            inDebounce = setTimeout(() => func.apply(context, args), delay);
        };
    }
    toggleTray() {
        this.showTray = !this.showTray;
    }
    handelMoreButtons(element, buttonWidths, buttons) {
        window.addEventListener('click', (e) => {
            let tray = this.moreTray;
            // @ts-ignore
            if (tray && (!tray.contains(e.target) || e.target.classList.contains('r-button__element'))) {
                this.showTray = false;
            }
        }, true);
        let leftSideWidth = element.querySelector('.r-bulk-actions__left').offsetWidth;
        this.buttonsInTray = [];
        let buttonsNotInTray = [];
        let visibleButtonTotalWidth = 0;
        let maxButtonWidth = Math.max(...buttonWidths);
        buttons.forEach((button, index) => {
            if (!button.classList.contains('r-bulk-actions__button-more')) {
                visibleButtonTotalWidth = visibleButtonTotalWidth + buttonWidths[index] + 8;
                if ((visibleButtonTotalWidth + maxButtonWidth) > leftSideWidth) {
                    this.buttonsInTray.push(button);
                }
                else {
                    buttonsNotInTray.push(button);
                }
            }
        });
        this.buttonsInTray.forEach((button) => {
            this.moreTray.appendChild(button);
        });
        buttonsNotInTray.forEach((button) => {
            element.querySelector('.r-bulk-actions__buttons-wrapper').appendChild(button);
        });
        if (this.buttonsInTray.length > 0) {
            element.querySelector('.r-bulk-actions__button-more').classList.add('r-bulk-actions__button-more--show');
        }
        else {
            element.querySelector('.r-bulk-actions__button-more').classList.remove('r-bulk-actions__button-more--show');
        }
    }
    render() {
        return (h("div", { class: {
                'r-bulk-actions': true,
                'r-bulk-actions--expanded': this.open,
                'overflow-visible': this.toggleOverflow,
                'overflow-hidden': !this.toggleOverflow,
            }, ref: (el) => this.bulkActionEl = el }, h("div", { class: "r-bulk-actions__wrapper" }, h("div", { class: "r-bulk-actions__left" }, h("div", { class: "r-bulk-actions__buttons-wrapper" }, h("slot", null)), h("div", { class: "r-bulk-actions__more" }, h("raul-button", { class: "r-bulk-actions__button-more", variant: "reverse", size: "small", onClick: () => this.toggleTray() }, "More"), h("div", { class: {
                'r-bulk-actions__more-tray': true,
                'r-bulk-actions__more-tray--show': this.showTray,
            }, ref: (el) => this.moreTray = el }))), h("div", { class: "r-bulk-actions__right" }, h("button", { type: "button", class: "r-bulk-actions__select-all", onClick: () => this.bulkActionsSelectAll.emit() }, "Select all ", this.totalRecords, " records"), h("span", { class: "r-bulk-actions__selected-count" }, this.selectedCount, " Selected"), h("button", { type: "button", class: "r-bulk-actions__close", onClick: () => this.closeBar() }, h("raul-icon", { icon: "close" }))))));
    }
    static get watchers() { return {
        "open": ["openChanged"]
    }; }
    static get style() { return "raul-bulk-action-bar{display:block}raul-bulk-action-bar .r-bulk-actions{bottom:0;color:#fff;left:0;position:fixed;width:100%;max-height:0;-webkit-transition:max-height .35s;transition:max-height .35s;z-index:1010}raul-bulk-action-bar .r-bulk-actions--expanded{max-height:3rem}\@media (min-width:768px){raul-bulk-action-bar .r-bulk-actions{position:relative;bottom:auto}}raul-bulk-action-bar .r-bulk-actions__wrapper{-ms-flex-align:center;align-items:center;display:-ms-flexbox;display:flex;-ms-flex-wrap:nowrap;flex-wrap:nowrap;position:relative;padding:.5rem;background-color:#0076cc;height:3rem}raul-bulk-action-bar .r-bulk-actions__left{display:-ms-flexbox;display:flex;margin-right:auto;-ms-flex-positive:1;flex-grow:1}raul-bulk-action-bar .r-bulk-actions__buttons-wrapper{display:inline-block}raul-bulk-action-bar .r-bulk-actions__buttons-wrapper .r-button{margin-right:.5rem}raul-bulk-action-bar .r-bulk-actions__more{display:inline-block;position:relative}raul-bulk-action-bar .r-bulk-actions__button-more{visibility:hidden}raul-bulk-action-bar .r-bulk-actions__button-more--show{visibility:visible}raul-bulk-action-bar .r-bulk-actions__right{-ms-flex-align:center;align-items:center;display:-ms-flexbox;display:flex;font-size:.75rem;-ms-flex-pack:end;justify-content:flex-end;-ms-flex:0 0 auto;flex:0 0 auto;max-width:265px;min-width:55px}raul-bulk-action-bar .r-bulk-actions__selected-count{font-size:.75rem}raul-bulk-action-bar .r-bulk-actions__select-all{padding-top:0;padding-bottom:0;padding-left:.75rem;padding-right:.75rem;display:none}raul-bulk-action-bar .r-bulk-actions__select-all:hover{text-decoration:underline}raul-bulk-action-bar .r-bulk-actions__select-all:after{display:inline-block;margin-left:.5rem;content:\"|\"}\@media only screen and (min-width:768px){raul-bulk-action-bar .r-bulk-actions__select-all{display:inline-block}}raul-bulk-action-bar .r-bulk-actions__close{padding:.75rem}raul-bulk-action-bar .r-bulk-actions__close raul-icon{font-size:.875rem}raul-bulk-action-bar .r-bulk-actions__more-tray{left:0;overflow:hidden;padding-top:.5rem;padding-left:.5rem;padding-right:0;padding-bottom:0;position:fixed;width:100%;background:#37474f;bottom:-100%;min-height:48px;-webkit-transition:bottom .35s;transition:bottom .35s;z-index:-1}raul-bulk-action-bar .r-bulk-actions__more-tray--show{bottom:48px}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button{margin-bottom:.5rem;margin-right:.5rem}\@media (min-width:768px){raul-bulk-action-bar .r-bulk-actions__more-tray{background-color:#fff;border-radius:.125rem;bottom:auto;display:none;padding-top:.5rem;padding-bottom:.5rem;padding-left:0;padding-right:0;position:absolute;right:auto;vertical-align:middle;-webkit-box-shadow:0 8px 16px 0 rgba(82,97,115,.18);box-shadow:0 8px 16px 0 rgba(82,97,115,.18);top:36px;width:180px;z-index:2000}raul-bulk-action-bar .r-bulk-actions__more-tray--show{display:block;bottom:auto}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button{display:block;margin-right:0}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button .r-button--reverse .r-button__content{-ms-flex-align:center;align-items:center;background-color:transparent;display:-ms-flexbox;display:flex;font-size:.75rem;font-weight:400;-ms-flex-pack:start;justify-content:flex-start;padding-top:0;padding-bottom:0;padding-left:1rem;padding-right:1rem;color:#37474f;min-height:32px}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button .r-button--reverse .r-button__focus-ring *{display:none}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button .r-button--reverse .r-button__element:hover~.r-button__content{background-color:transparent;text-decoration:underline}raul-bulk-action-bar .r-bulk-actions__more-tray raul-button .r-button--reverse .r-button__content:before{border-color:#0076cc!important;border-radius:0!important;top:2px!important;right:2px!important;bottom:2px!important;left:2px!important}}"; }
};

export { RaulBulkActionBar as raul_bulk_action_bar };
