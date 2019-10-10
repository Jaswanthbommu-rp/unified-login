/*jshint esversion: 6 */

let loadScript = (url) => {
    return new Promise((resolve, reject) => {
        let script = document.createElement('script');
        script.src = url;
        document.body.appendChild(script);

        script.onload = () => {
            resolve(true);
        };

        script.onerror = () => {
            reject(false);
        };
    });
};


let loadFiles = (files) => {
    let coreScripts = ['/home/Assets/build/js/vendor.js', '/home/Assets/build/js/core.js'];
    files = coreScripts.concat(files);

    return files.reduce((promiseChain, file) => {
        return promiseChain.then(result => {
            return loadScript(file);
        });
    }, Promise.resolve());
};