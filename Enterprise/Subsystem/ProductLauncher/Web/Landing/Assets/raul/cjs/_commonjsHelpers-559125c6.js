'use strict';

var commonjsGlobal = typeof globalThis !== 'undefined' ? globalThis : typeof window !== 'undefined' ? window : typeof global !== 'undefined' ? global : typeof self !== 'undefined' ? self : {};

function createCommonjsModule(fn, module) {
	return module = { exports: {} }, fn(module, module.exports), module.exports;
}

function getCjsExportFromNamespace (n) {
	return n && n['default'] || n;
}

exports.commonjsGlobal = commonjsGlobal;
exports.createCommonjsModule = createCommonjsModule;
exports.getCjsExportFromNamespace = getCjsExportFromNamespace;
