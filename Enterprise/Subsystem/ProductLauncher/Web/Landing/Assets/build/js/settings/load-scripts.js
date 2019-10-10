'use strict';

/*jshint esversion: 6 */

var loadScript = function loadScript(url) {
    return new Promise(function (resolve, reject) {
        var script = document.createElement('script');
        script.src = url;
        document.body.appendChild(script);

        script.onload = function () {
            resolve(true);
        };

        script.onerror = function () {
            reject(false);
        };
    });
};

var loadFiles = function loadFiles(files) {
    var coreScripts = ['/home/Assets/build/js/vendor.js', '/home/Assets/build/js/core.js'];
    files = coreScripts.concat(files);

    return files.reduce(function (promiseChain, file) {
        return promiseChain.then(function (result) {
            return loadScript(file);
        });
    }, Promise.resolve());
};