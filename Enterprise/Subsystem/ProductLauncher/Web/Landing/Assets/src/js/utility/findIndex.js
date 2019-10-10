/**
    findIndex - just find the index of an item in an array 
        - doesn't require jQuery
        - runs backward -> iterates slightly faster than forward

    TODO: add forward iterating for loop - to allow for starting from beginning
**/
(function(){
    var _UTILS = window._UTILS || (window._UTILS = {});

    _UTILS.findIndex = function(array, item){
        var i = array.length;

        for (i; i--;){
            if (array[i].value === item){
                return i;
            }
        }
    }
    
})()