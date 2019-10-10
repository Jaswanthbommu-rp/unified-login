function checkHexCode(hex){

    if (hex[0] !== '#') hex.unshift('#');

    return /(^#[0-9A-F]{6}$)|(^#[0-9A-F]{3}$)/i.test(hex);
}

(function(){
    var _UTILS = window._UTILS || (window._UTILS = {});

    _UTILS.checkHexCode = function(hex){
        if (hex[0] !== '#') hex.unshift('#');

        return /(^#[0-9A-F]{6}$)|(^#[0-9A-F]{3}$)/i.test(hex);
    }
})()