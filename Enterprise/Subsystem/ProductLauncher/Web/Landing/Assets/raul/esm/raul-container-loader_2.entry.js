import { r as registerInstance, h } from './core-9263a98c.js';

const Dot = (props) => h("div", { class: "r-container-loader__dot", style: { animationDelay: props.animationDelay } });
const RaulcontainerLoader = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("div", { class: 'r-container-loader__container' }, h("div", { class: 'r-container-loader__dots-container' }, Array.from({ length: 3 }).map((_, index) => h(Dot, { animationDelay: `${index * 333}ms` })))));
    }
    static get style() { return "raul-container-loader{width:100%;height:100%}raul-container-loader .r-container-loader__container{position:absolute;top:0;right:0;bottom:0;left:0;width:100%;height:100%;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center;background:hsla(0,0%,100%,.8)}raul-container-loader .r-container-loader__container .r-container-loader__dots-container{display:-ms-flexbox;display:flex;-ms-flex-direction:row;flex-direction:row;-ms-flex-pack:justify;justify-content:space-between;opacity:1;width:50px}raul-container-loader .r-container-loader__container .r-container-loader__dot{background-color:#c6ccd0;height:10px;width:10px;border-radius:50%;-webkit-animation:color_change 1s linear infinite;animation:color_change 1s linear infinite}\@-webkit-keyframes color_change{0%{background-color:#0076cc}20%{background-color:#0076cc}50%{background-color:#c6ccd0}to{background-color:#c6ccd0}}\@keyframes color_change{0%{background-color:#0076cc}20%{background-color:#0076cc}50%{background-color:#c6ccd0}to{background-color:#c6ccd0}}"; }
};

const RaulPageLoader = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("div", { class: 'r-page-loader__container' }, h("div", { class: 'r-page-loader__spinner' })));
    }
    static get style() { return "raul-page-loader .r-page-loader__container{z-index:10000;position:fixed;top:0;right:0;bottom:0;left:0;height:100vh;width:100vw;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center;background:rgba(55,71,79,.5)}raul-page-loader .r-page-loader__container .r-page-loader__spinner{opacity:1;height:3rem;width:3rem;border-radius:9999px;border-width:4px;border-style:solid;border-color:#fff;border-right:4px solid #0076cc;-webkit-animation:spin 1s linear infinite;animation:spin 1s linear infinite}\@-webkit-keyframes spin{0%{-webkit-transform:rotate(0deg);transform:rotate(0deg)}to{-webkit-transform:rotate(1turn);transform:rotate(1turn)}}\@keyframes spin{0%{-webkit-transform:rotate(0deg);transform:rotate(0deg)}to{-webkit-transform:rotate(1turn);transform:rotate(1turn)}}"; }
};

export { RaulcontainerLoader as raul_container_loader, RaulPageLoader as raul_page_loader };
