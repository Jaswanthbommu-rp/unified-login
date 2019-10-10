/**
  IN DEV DON'T USE YET - 10/3/17 by henry hedges

  filterTableItems - a function to filter items in a table based on a hard match.

  For this to work: 
    1) search input will need this class => ''
    2) search input will need this attribute => data-searchthis="#your-table-id"
    3) the table you are searching will need an id that matches "#your-table-id"
    4) each td or element to search will need this attribute => data-search="yourvalue"
    5) paste this at the bottom of the page with the search input => searchInit();
**/

(function(){

  //create UTILS object
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

  // search function
  _UTILS.filterTableItems = function (input) {
    var td, i,
        filter = input.value.toUpperCase(),
        table = document.querySelector(input.dataset.target),
        tr = table.getElementsByTagName('tr'),
        trLen = tr.length;

    for (i = 0; i < trLen; i++) {
      td = tr[i].getElementsByTagName("td")[0];
      if (td) {
        //TODO: change this to access the main value of the row
        if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
          tr[i].style.display = "";
        } else {
          tr[i].style.display = "none";
        }
      }
    }
  }
  
  _UTILS.initSearch = function(){  
    _UTILS.forAll( document.querySelectorAll('.expand-search'), function(element, idx){
      // event listener for search on 'keyup'
      element.querySelector('input').addEventListener('keyup', function(e){
        _UTILS.filterTableItems(e.target);
      })

    }, false)
  }

  _UTILS.initSearch();

})()
