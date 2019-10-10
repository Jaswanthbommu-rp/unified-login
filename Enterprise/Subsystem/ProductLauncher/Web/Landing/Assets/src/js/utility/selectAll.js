/**
    created: 10/4/17 - Henry Hedges

This function supports multiple select-alls on the same page. It uses an identifier class you create to differentiate which boxes are supposed to be checked.

To use: 
    1) to your main select-all inputs, add the class 'select-all' 
    2) to your main select-all inputs, add the attribute data-selectall=".your-identifier-class-here"
    3) to the dependent inputs, add the identifier class

For Example:
    Main select-all input:
        <label class="md-check dark-bluebox">
            <input type="checkbox" class="has-value select-all" data-selectall=".company-property">
            <i class="primary"></i>
        </label>

    Dependent inputs:
        <label class="md-check dark-bluebox">
            <input type="checkbox" class="has-value company-property">
            <i class="primary"></i>
        </label>

**/
(function(){
    var _UTILS = window._UTILS || (window._UTILS = {});

    if (!_UTILS.forAll){
        _UTILS.forAll = function (array, callback, ordered){
          var i = array.length,
              len = i;

          // use ordered = true if the order of what you are doing is important (i.e. appending elements)
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
    }

    _UTILS.initSelectAll = function(){
        _UTILS.forAll( document.querySelectorAll('.select-all'), function(input){

            input.addEventListener('change',function(e){
                var allElements = $( document.querySelectorAll(this.dataset.selectall) );

                if (this.checked){
                    allElements.prop('checked', true);
                } else {
                    allElements.prop('checked', false);
                }
            })

        })
    }

    _UTILS.initSelectAll();

})()
