/**
    forAll - forEach but doesn't include object support and runs backward/forward depending on use

**/
(function(){
    var _UTILS = window._UTILS || (window._UTILS = {});

    _UTILS.forAll = function(array, callback, ordered){
        var i = array.length,
            len = i;

        // use ordered = true if the order of what you are doing is important (i.e. appending elements)Native
        if (ordered){
            for(i = 0; i<len; i++){
                callback(array[i], i);
            }
        } else {
            for (i; i--;){
                callback(array[i], i);
            }
        }

    }
})();
