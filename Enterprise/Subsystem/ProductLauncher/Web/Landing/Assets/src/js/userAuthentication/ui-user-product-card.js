
var buildProductCard = function(template, id, personaId, name, description, productUrl, newTab, learnMore, subsolution, isFavorite, hasAccess, parentContainer) {

    var subsolutionElement = $(template).find('.product-card-subsolution');
    $(template).removeClass('rp-product-card-template');
    $(template).removeClass('hidden');

    $(template).find('.product-card').click(function(e) {
        var target = $(event.target);
        if(target.is('.product-card-favorite') || target.is('.product-tooltip-icon .fa-info-circle')) {
            e.preventDefault();
            return;
        }
        //Send the SAML information for the user on the application
        e.preventDefault();
        sendSamlData(id,personaId);
    });

    $(template).find('.product-card-favorite').click(function(e) {
        //Check if product will ve fave
        var isFave = !$(template).find('.product-card-favorite').hasClass('active');

        var data = {};
        data.name = 'IsFavorite';
        data.productId = id;

        if(isFave){
            data.value = 1;
            //Call to favorite this product
            userAuthAPIService('PUT','api/personas/products/' + id + '/productSettings?name=IsFavorite&value=1', data, 'updatedFavorite');
            $(template).find('.product-card-favorite').addClass('active');
            updateProductsSession(id, true);
        } else {
            data.value = 0;
            //Call to favorite this product
            userAuthAPIService('PUT','api/personas/products/' + id + '/productSettings?name=IsFavorite&value=0', data, 'updatedFavorite');
            $(template).find('.product-card-favorite').removeClass('active');
            updateProductsSession(id, false);
        }
    });

    $(template).find('.product-card-icon').attr('src', getProductIcon(id));

    $(template).find('.product-card-title').html(name);

    if(subsolution) {
        subsolutionElement.html(subsolution);
    } else {
        subsolutionElement.hide();
    }

    $(template).find('.tooltip-product-card-title').html(name);
    $(template).find('.tooltip-product-card-description').html(description);

    $(template).find('.tooltip-product-card-learn-more').attr('href', learnMore);

    $(template).find('.product-tooltip-icon').attr('data-tooltip-content','#ap-tooltip-content-' + id);
    $(template).find('#ap-tooltip-content').attr('id','ap-tooltip-content-' + id);

    if (newTab) {
        $(template).find('.tooltip-product-card-learn-more').attr('target', '_blank');
    }

    if(hasAccess !== "" && hasAccess === false){
        $(template).find('.product-card').prepend('<div class="disabled-product-card"></div>');
    }

    if(isFavorite !== "" && isFavorite === true){
        $(template).find('.product-card-favorite').addClass('active');
    }

    parentContainer.append(template);
    $(template).find('[ui-jp]').uiJp();

};

var updateProductsSession = function(id, value){
    var profileDetails = $.sessionStorage.get("profileDetails");

    //PULL THE PRODUCTS
    var products = profileDetails.assignedProducts;

    for(i=0; i < products.length; i++){
        if(products[i].productId === id){
            products[i].isFavorite = value;
        }
    }

    buildAppPicker(products);
    profileDetails.assignedProducts = products;
    $.sessionStorage.set("profileDetails",profileDetails);
    console.log(products);
};

var updatedFavorite = function(response){
    console.log("updated favorite for product");
    console.log(response);
};

var sendSamlData = function(productId, personaId){
    //Send the SAML information for the user on the application
    window.open('/saml?productId=' + productId + '&personaId=' + personaId);
};
