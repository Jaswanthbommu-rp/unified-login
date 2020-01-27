'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsDevice = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("div", { class: "r-docs-device" }, core.h("div", { class: "r-docs-device-content" }, core.h("slot", null))));
    }
    static get style() { return ".r-docs-device{border-color:#f7f8f9;position:relative;width:375px;height:667px;background:#fff;border-width:16px;border-top-width:60px;border-bottom-width:60px;border-radius:36px;-webkit-box-shadow:0 0 0 1px #ebedee;box-shadow:0 0 0 1px #ebedee}.r-docs-device:before{width:60px;height:5px;top:-30px;border-radius:10px}.r-docs-device:after,.r-docs-device:before{content:\"\";display:block;position:absolute;left:50%;-webkit-transform:translate(-50%,-50%);transform:translate(-50%,-50%);background-color:#ebedee}.r-docs-device:after{width:35px;height:35px;bottom:-65px;border-radius:50%}.r-docs-device .r-docs-device-content{padding:1rem;border-width:1px;border-color:#ebedee;height:100%;background:#fff}"; }
};

exports.docs_device = DocsDevice;
