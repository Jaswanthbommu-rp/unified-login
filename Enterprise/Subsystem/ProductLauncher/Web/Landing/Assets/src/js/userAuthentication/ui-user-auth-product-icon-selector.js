var getProductIcon = function (productId){

    var productIconPath = "https://cdn.realpage.com/styles/v2.0/icons/product/";

    switch (productId) {
        case 1:
            //Onesite
            productIconPath += "rpi-hq.svg";
            break;
        case 4:
            //Asset Optimization
            productIconPath += "house-with-chart.svg";
            break;
        case 6:
            //Lead2Lease
            productIconPath += "square-with-L2L.svg";
            break;
        case 7:
            //YieldStar
            productIconPath += "folder-1.svg";
            break;
        case 8:
            //RealPage Accounting
            productIconPath += "calculator-buttons.svg";
            break;
        case 9:
            //Websites & Syndication
            productIconPath += "monitor-with-www.svg";
            break;
        case 10:
            //Prospect Contact Center
            productIconPath += "dollar-sign-headset.svg";
            break;
        case 13:
            //Spend Management
            productIconPath += "cart-with-gear.svg";
            break;
        case 15:
            //Renters Insurance
            productIconPath += "house-on-shield.svg";
            break;
        case 14:
            productIconPath += "toolbox.svg";
            break;
        case 16:
            //Vendor Services
            productIconPath += "people-desk-dollar.svg";
            break;
        case 17:
            //Active Building
            productIconPath += "monitor-with-user.svg";
            break;
        case 18:
            //Utility Management
            productIconPath += "bulb-2.svg";
            break;
        case 19:
            productIconPath += "education-online.svg";
            break;
        case 20:
            //RealPage Document Management
            productIconPath += "folder-document.svg";
            break;
        default:
            //OneSite Conversions
            productIconPath += "docs-with-checkmark.svg";
    }

    //console.log("Product Name: " + productName);
    //console.log("Product Icon Path: " + productIconPath);
    return productIconPath;

};