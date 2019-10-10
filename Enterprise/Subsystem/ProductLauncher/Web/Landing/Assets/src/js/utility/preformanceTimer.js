/**

    created: 10/4/17 - henry hedges

    This is a performance timer. 

    To use it all you need to do is pass a function into perfTimer and it will give you an estimate of how long the operation took;

**/
(function(){
    var _UTILS = window._UTILS || (window._UTILS = {})
    if (!_UTILS.perfTimer){
        _UTILS.perfTimer = function(func){
            var perftimer = Date.now();
            var func = func || function(){};

            func();

            perftimer = Date.now() - perftimer;

            console.log('PERFORMANCE TIMER SELECT ALL:: ',perftimer);
        }
    }
})()

