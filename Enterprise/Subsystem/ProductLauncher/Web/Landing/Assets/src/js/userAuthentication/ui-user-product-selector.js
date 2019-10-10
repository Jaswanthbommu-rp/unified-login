
//When a product is selected, it will call the function "selectedProduct" with the product ID
var rpGetProducts = function(response){
    console.log("PRODUCT OPTIONS",response);

    $.each(response.data, function (i, item) {
        console.log(item.familyId);
        var productFamily = $('#product-family-template').clone();
        $(productFamily).removeAttr('id');
        $(productFamily).find('li[data-target="#product-family-sub-menu"]').attr('data-target',"#"+item.title.replace(/\s/g, ''));
        $(productFamily).find('#product-family-sub-menu').attr('id',item.title.replace(/\s/g, ''));
        $(productFamily).find('.assign-product-family-title').html(item.title);
        $(productFamily).find('.rp-select-all input[type=checkbox]').attr('value',item.familyId);
        $(productFamily).css('display','block');

        $.each(item.solutions, function (x, solution) {
            var productSolution = $('#product-solution-template').clone();
            $(productSolution).removeClass('hidden');
            $(productSolution).find('.assign-product-solution-title').html(solution.titleId);
            $(productSolution).find('.assign-product-solution-products').html(solution.products);
            $(productSolution).find('input[type=checkbox]').attr('value',solution.solutionId);
            if(solution.isAssigned === true){
                console.log("Found a solution: " + solution.isAssigned);
                $(productSolution).find('input[type=checkbox]').prop('checked',solution.isAssigned);
            }

            $(productSolution).click(function(e) {
                console.log("EVENT: " , e);
                if(e.target.localName === 'i' || e.target.localName === 'input'){
                    console.log("CHECKBOX EVENT: " , e.target);
                    return;
                } else {
                    console.log("CONTAINER EVENT: " , e.target);
                    $('.assign-product-solution').removeClass('selected');
                    $(this).addClass('selected');
                    selectedProduct(solution.titleId);
                }
            });

            /*$(productSolution).on('click', '.md-check input[type=checkbox]', function( e ) {
                console.log("EVENT: " , e);
                e.stopPropagation();
            });*/

            $(productFamily).find('#'+item.title.replace(/\s/g, '')).append(productSolution);

            if(i == 0 && x == 0){
                $(productFamily).find('.collapse').addClass('in');
                $(productFamily).find('li:first-child').removeClass('collapsed');
                $(productSolution).click();
            }
        });

        $('#product-menu-list').append(productFamily);
    });

    $(document).on('click', '#product-menu-list .md-check input[type=checkbox]', function( e ) {
        e.stopPropagation();

        if ( $(this).parents(".rp-select-all").length === 1 ) {
            var checkboxState = $(this).prop('checked');
            var productsToSelect = $(this).closest('ul').find('.sub-menu').find('.md-check input[type=checkbox]');

            $.each( productsToSelect, function( key, value ) {
                if($(value).prop('checked') !== checkboxState){
                    $(value).prop('checked',checkboxState);
                }
            });

        }

    });

    var $productMenu = $('#product-menu-list');
    $productMenu.on('show.bs.collapse','.collapse', function() {
        $productMenu.find('.collapse.in').collapse('hide');
    });

};