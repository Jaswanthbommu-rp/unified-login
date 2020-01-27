'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulModal = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        /**
        * A `normal` modal will have a the header and body centered and no close button. A `media` modal will have the header content aligned to the left and a close button.
        */
        this.variant = 'normal';
        /**
        * Determines wether the modal can be closed via clicking away or the `Escape` key
        */
        this.dismissable = true;
        this.modalClose = core.createEvent(this, "modalClose", 7);
    }
    componentDidLoad() {
        this.overlay.focus();
        if (this.dismissable) {
            this.overlay.addEventListener('click', (e) => {
                // @ts-ignore
                if (!this.modal.contains(e.target))
                    this.modalClose.emit();
            }, true);
            window.addEventListener('keydown', e => {
                if (['Esc', 'Escape'].includes(e.key))
                    this.modalClose.emit();
            });
        }
    }
    render() {
        return (core.h("div", { class: {
                "r-modal__overlay": true,
                "r-modal__overlay--media": this.variant === 'media'
            }, tabindex: "-1", ref: el => this.overlay = el }, core.h("div", { ref: el => this.modal = el, class: {
                "r-modal__container": true,
                "r-modal__container--normal": this.variant === 'normal',
                "r-modal__container--media": this.variant === 'media'
            }, role: "dialog", "aria-modal": "true" }, core.h("slot", null))));
    }
    static get style() { return "raul-modal .r-modal__overlay{display:-ms-flexbox;display:flex;position:fixed;top:0;right:0;bottom:0;left:0;-ms-flex-align:center;align-items:center;-ms-flex-pack:center;justify-content:center;z-index:10000;padding:2.5rem;height:100%;-webkit-animation:mfadeIn .3s cubic-bezier(0,0,.2,1);animation:mfadeIn .3s cubic-bezier(0,0,.2,1);will-change:transform;background:rgba(0,0,0,.6)}\@media (max-width:640px){raul-modal .r-modal__overlay{-ms-flex-align:end;align-items:flex-end}}raul-modal .r-modal__overlay .r-modal__container{background-color:#fff;padding-top:2rem;padding-bottom:2rem;padding-right:1rem;padding-left:1rem;border-radius:.25rem;display:-ms-flexbox;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:start;justify-content:flex-start;display:flex;-ms-flex:0 1 auto;flex:0 1 auto;-webkit-animation:mslideIn .3s cubic-bezier(0,0,.2,1);animation:mslideIn .3s cubic-bezier(0,0,.2,1);will-change:transform;max-height:calc(100vh - 80px)}raul-modal .r-modal__overlay .r-modal__container raul-modal-header{-ms-flex:0 1 auto;flex:0 1 auto}raul-modal .r-modal__overlay .r-modal__container .r-modal__header__container{display:-ms-flexbox;display:flex;-ms-flex-direction:row;flex-direction:row;-ms-flex-pack:justify;justify-content:space-between}raul-modal .r-modal__overlay .r-modal__container--normal{width:360px}raul-modal .r-modal__overlay .r-modal__container--normal .r-modal__header__items{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:start;justify-content:flex-start;-ms-flex-align:center;align-items:center;text-align:center;-ms-flex:1 1 0%;flex:1 1 0%}raul-modal .r-modal__overlay .r-modal__container--normal .r-modal__header__items raul-content h2{margin-bottom:0}raul-modal .r-modal__overlay .r-modal__container--normal .r-modal__header__items raul-content p{margin-bottom:.5rem;opacity:.8}raul-modal .r-modal__overlay .r-modal__container--normal .r-modal__header__close-button{display:none}raul-modal .r-modal__overlay .r-modal__container--normal raul-modal-body{display:-ms-flexbox;display:flex;-ms-flex:1 1 0%;flex:1 1 0%;-ms-flex-direction:column;flex-direction:column;text-align:center;overflow-y:auto;margin-bottom:1rem;-ms-flex:1 1 auto!important;flex:1 1 auto!important;scrollbar-width:thin;scrollbar-color:#d3d3d3 #fff;scrollbar-base-color:#theme \"colors.gray-light\";scrollbar-face-color:#aaa;scrollbar-3dlight-color:#fff;scrollbar-track-color:#fff;scrollbar-arrow-color:#fff;scrollbar-shadow-color:#fff;scrollbar-dark-shadow-color:#fff;-ms-overflow-style:-ms-autohiding-scrollbar}raul-modal .r-modal__overlay .r-modal__container--normal raul-modal-body .r-modal__body__container{width:100%;padding-left:1rem;padding-right:1rem}raul-modal .r-modal__overlay .r-modal__container--normal raul-modal-body .r-modal__body__container raul-content .r-content{opacity:.8}raul-modal .r-modal__overlay .r-modal__container--normal raul-modal-body::-webkit-scrollbar{width:.25rem}raul-modal .r-modal__overlay .r-modal__container--normal raul-modal-body::-webkit-scrollbar-thumb{background-color:#c6ccd0;height:8rem;opacity:.8}raul-modal .r-modal__overlay .r-modal__container--media{width:704px}\@media (max-width:640px){raul-modal .r-modal__overlay .r-modal__container--media{width:100%;height:100%}}raul-modal .r-modal__overlay .r-modal__container--media .r-modal__header__items{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:start;justify-content:flex-start;-ms-flex-align:center;align-items:center}raul-modal .r-modal__overlay .r-modal__container--media .r-modal__header__items raul-content h2{margin-bottom:0}raul-modal .r-modal__overlay .r-modal__container--media .r-modal__header__items raul-content p{margin-bottom:.5rem;opacity:.8}raul-modal .r-modal__overlay .r-modal__container--media .r-modal__header__close-button{font-size:1.25rem;color:#9ba3a7;width:1.5rem;height:1.5rem;-ms-flex:0 1 auto;flex:0 1 auto;cursor:pointer}raul-modal .r-modal__overlay .r-modal__container--media .r-modal__header__close-button:focus{outline:none}raul-modal .r-modal__overlay .r-modal__container--media .r-modal__header__close-button:focus .r-modal__header__close-button__focus-utility{outline:1px solid #0076cc}raul-modal .r-modal__overlay .r-modal__container--media .r-modal__header__close-button .r-modal__header__close-button__focus-utility{width:100%;height:100%;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center}raul-modal .r-modal__overlay .r-modal__container--media .r-modal__header__close-button .r-modal__header__close-button__focus-utility:focus{outline:none}raul-modal .r-modal__overlay .r-modal__container--media raul-modal-body{-ms-flex:1 1 auto!important;flex:1 1 auto!important;overflow:hidden}raul-modal .r-modal__overlay .r-modal__container--media raul-modal-body .r-modal__body__container{display:-ms-flexbox;display:flex;-ms-flex:1 1 0%;flex:1 1 0%;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center}raul-modal .r-modal__overlay .r-modal__container--media raul-modal-body .r-modal__body__container img{-ms-flex:1 1 0%;flex:1 1 0%}raul-modal .r-modal__overlay .r-modal__container raul-modal-footer .r-modal__footer{display:-ms-flexbox;display:flex;-ms-flex-direction:row;flex-direction:row;margin-top:1rem;padding-left:1rem;padding-right:1rem}\@media (max-width:640px){raul-modal .r-modal__overlay .r-modal__container raul-modal-footer .r-modal__footer{-ms-flex-direction:column;flex-direction:column}raul-modal .r-modal__overlay .r-modal__container raul-modal-footer .r-modal__footer raul-button{padding:0}raul-modal .r-modal__overlay .r-modal__container raul-modal-footer .r-modal__footer raul-button:not(:first-child){padding-top:1rem}}\@media (min-width:640px){raul-modal .r-modal__overlay .r-modal__container raul-modal-footer .r-modal__footer raul-button{-ms-flex:1 1 0%;flex:1 1 0%}raul-modal .r-modal__overlay .r-modal__container raul-modal-footer .r-modal__footer raul-button:not(:first-child){padding-left:1rem}}raul-modal .r-modal__overlay--media{padding:0}raul-modal .r-modal__overlay--media .r-modal__container--media{border-radius:.25rem;max-height:100%;padding-left:2rem;padding-right:2rem}raul-modal .r-modal__overlay--media .r-modal__container--media raul-modal-body{-ms-flex:1 1 0%;flex:1 1 0%}\@-webkit-keyframes mfadeIn{0%{opacity:0}to{opacity:1}}\@keyframes mfadeIn{0%{opacity:0}to{opacity:1}}\@-webkit-keyframes mslidein{0%{-webkit-transform:translatey(15%);transform:translatey(15%)}to{-webkit-transform:translatey(0);transform:translatey(0)}}\@keyframes mslidein{0%{-webkit-transform:translatey(15%);transform:translatey(15%)}to{-webkit-transform:translatey(0);transform:translatey(0)}}\@media (-ms-high-contrast:none),screen and (-ms-high-contrast:active){raul-modal .r-modal__container,raul-modal raul-modal-body{-ms-flex:auto;flex:auto}raul-modal raul-modal-body raul-content{max-height:calc(100vh - 300px)}}"; }
};

const RaulModalBody = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("div", { class: "r-modal__body__container" }, core.h("slot", null)));
    }
};

const RaulModalFooter = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("div", { class: 'r-modal__footer' }, core.h("slot", null)));
    }
};

const RaulModal$1 = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.modalClose = core.createEvent(this, "modalClose", 7);
    }
    render() {
        return (core.h("div", { class: 'r-modal__header__container' }, core.h("div", { class: 'r-modal__header__items' }, core.h("raul-content", null, core.h("h2", { class: 'font-xl' }, this.modalTitle), core.h("p", null, this.description))), core.h("div", { class: 'r-modal__header__close-button', role: 'button', tabindex: "0", onClick: () => this.modalClose.emit(), onKeyDown: e => {
                if (e.key === 'Enter')
                    this.modalClose.emit();
            } }, core.h("div", { class: 'r-modal__header__close-button__focus-utility', tabindex: "-1" }, core.h("raul-icon", { icon: "close" })))));
    }
};

exports.raul_modal = RaulModal;
exports.raul_modal_body = RaulModalBody;
exports.raul_modal_footer = RaulModalFooter;
exports.raul_modal_header = RaulModal$1;
