// change widget status text from active to inactive and vice versa
// Add class 'active-inactive-switch' to the switch input element, then add data-switch="your-identifier-here"
// Then add class 'active-inactive-switch-text' and class 'your-identifier-here' to the text element accompanying your switch

// old function - used in com widget page -> update this to use new _UTILS function

$('.active-inactive-switch').change(function(e){
    console.log('make sure to switch this over to use _UTILS.initActiveInactiveSwitch ')
    // var $this = $(this),
    //     textElm = $('.active-inactive-switch-text.' + $this.data().switch);

    // // can only be 'active' or 'inactive'
    // if (textElm.text() === 'Inactive') {
    //     textElm.text('Active');
    // } else {
    //     textElm.text('Inactive');
    // };
});

/**
    created: 10/4/17 - henry hedges

    Change text for active-inactive switch.

    The general HTML layout can be whatever you want - just make sure to add to your switch input -> class="a-i-switch" data-target="#your-text-element-target-here". 
    You will need to add a corresponding ID to the text element.

    Example:

        Switch input:
            <label class="ui-switch success m-t-xs m-r">
                <input type="checkbox" class="has-value a-i-switch" data-target="#edit-ts-a-i-switch-text">
                <i></i>
            </label>

        Text element:
            <div id="edit-ts-a-i-switch-text" style="display: table; padding: 2px;"> Inactive </div>
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
    
    _UTILS.initActiveInactiveSwitch = function(){
        _UTILS.forAll( document.querySelectorAll('.a-i-switch'), function(item){
            item.addEventListener('change',function(e){
                var textTarget = document.querySelector(this.dataset.target);

                if (this.checked){
                    $('input[name="'+ $(this).attr('name') +'"]').attr('checked',true);
                    textTarget.innerText = 'Active';
                } else {
                    $('input[name="'+ $(this).attr('name') +'"]').attr('checked',false);
                    textTarget.innerText = 'Inactive';
                }
            })  
        })

    }

    _UTILS.initActiveInactiveSwitch();
})()