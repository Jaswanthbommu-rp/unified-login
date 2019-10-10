var buildAppPicker = function(products){

    //Clear the All Products section
    $('.app-switcher-services').not(':first').remove();
    //Clear the Favorites section
    $('.app-switcher-favorites .app-switcher-product').remove();

    $.each(products, function (productIndex, product) {

        //Initialize the Parent Container
        var parentContainer;

        if(product.family !== null && $('div[data-family="'+ product.family +'"]').length > 0){
            //console.log("PARENT ALREADY CREATED: " + product.family);
            parentContainer = $('div[data-family="'+ product.family +'"]');
        } else if (product.family === null && $('div[data-family="Miscellaneous"]').length > 0) {
            //console.log("PARENT ALREADY CREATED: " + product.family);
            parentContainer = $('div[data-family="Miscellaneous"]');
        } else {
            //console.log("CREATING PARENT: " + product.family);
            parentContainer = $('.app-switcher-service-template').clone();
            $(parentContainer).attr('data-family',product.family);
            $(parentContainer).removeClass('app-switcher-service-template');
            $(parentContainer).removeClass('hidden');
            if(product.family === null || product.family === undefined ){
                $(parentContainer).find('.app-switcher-service-title').html("Miscellaneous");
            } else {
                $(parentContainer).find('.app-switcher-service-title').html(product.family);
            }

            $('.app-switcher-services').append(parentContainer);
        }

        //console.log("ADDING PRODUCT: " + product.productName);
        var productTemplate = $(parentContainer).find('.app-switcher-product-template').clone();
        $(productTemplate).removeClass('app-switcher-product-template');
        $(productTemplate).removeClass('hidden');

        $(productTemplate).find('.app-switcher-product-title').click(function(e) {
            //Send the SAML information for the user on the application
            sendSamlData(product.productId,product.personaId);
        });

        $(productTemplate).find('.app-switcher-product-link').click(function(e) {
            e.preventDefault();
            //Send the SAML information for the user on the application
            sendSamlData(product.productId,product.personaId);
        });

        $(productTemplate).find('.app-switcher-product-icon').attr('src', getProductIcon(product.productId));
        $(productTemplate).find('.app-switcher-product-title').html(product.productName);

        if(product.isFavorite){
            var favorite = $(productTemplate).clone();
            $('.app-switcher-favorites').append(favorite);
        }

        $(parentContainer).append(productTemplate);

    });

};
