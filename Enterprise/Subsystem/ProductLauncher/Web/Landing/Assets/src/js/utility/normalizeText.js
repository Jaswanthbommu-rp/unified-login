function normalizeText(string){
    return string.replace(/\s/g,'').replace(/&/g,'and').toLowerCase();
}

(function(){
    var _UTILS = window._UTILS || (window._UTILS = {});

    _UTILS.normalizeText = function(string){
        return string.replace(/\s/g,'').replace(/&/g,'and').toLowerCase();
    }
    
})()