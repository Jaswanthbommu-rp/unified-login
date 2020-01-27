'use strict';

function randomUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        const r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
function stripBaseIndent(str) {
    const numSpaces = /^[^\r\n]*?(?=\S)/m.exec(str)[0].length, baseIndent = new RegExp(`^\\s{${numSpaces}}`, 'gm');
    return str.split(baseIndent).join('\n').trim();
}

exports.randomUID = randomUID;
exports.stripBaseIndent = stripBaseIndent;
