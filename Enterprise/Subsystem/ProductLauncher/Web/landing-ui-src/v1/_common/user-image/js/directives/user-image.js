(function (angular) {
    "use strict";

    function userImageDirective() {
        function link(scope, elem, attr) {
            var dir = {},
                defaultStyles = {
                    size: "50px",
                    borderRadius: "50%"
                };

            dir.init = function() {
                scope.imageWrapperStyle = {};

                dir.destroyWatch = scope.$on("$destroy", dir.destroy);

                dir.setImageStyles(false); //initialize image style
                dir.imgElem = elem.children(); //get image element

                if(dir.imgElem.is("img")) {
                    if(dir.isAttrEmpty(scope.imageSrc)) {
                        // logc("No image supplied, replacing with default");
                        dir.updateFallback(false);
                    }

                    dir.imgElem.on("load", dir.imageOnLoad);
                    dir.imgElem.on("error", dir.imageOnError);
                }                
            };

            dir.destroy = function() {
                scope.imageWrapperStyle = undefined;

                dir.destroyWatch();
                dir.destroyWatch = undefined;

                dir.imgElem.off();
                dir.imgElem = undefined;

                dir = undefined;
            };

            dir.initImageSize = function() {
            };


            dir.setHeight = function(height, defaultVal) {
                if(!dir.isAttrEmpty(height)) {
                    scope.imageWrapperStyle["height"] = height;                    
                } else if(angular.isDefined(defaultVal)) {
                    scope.imageWrapperStyle["height"] = defaultVal;
                }
            };

            dir.setWidth = function(width, defaultVal) {
                if(!dir.isAttrEmpty(width)) {
                    scope.imageWrapperStyle["width"] = width;
                } else if(angular.isDefined(defaultVal)) {
                    scope.imageWrapperStyle["width"] = defaultVal;
                }
            };

            dir.setIconSize = function(iconSize, defaultVal) {
                if(!dir.isAttrEmpty(iconSize)) {
                    scope.imageWrapperStyle["font-size"] = iconSize;
                } else if(angular.isDefined(defaultVal)) {
                    scope.imageWrapperStyle["font-size"] = defaultVal;
                } else {
                    scope.imageWrapperStyle["font-size"] = scope.imageWrapperStyle.width;
                }
            };

            dir.setBorderRadius = function(radius, defaultVal) {
                if(!dir.isAttrEmpty(radius)) {
                    scope.imageWrapperStyle["border-radius"] = radius;
                } else if(angular.isDefined(defaultVal)) {
                    scope.imageWrapperStyle["border-radius"] = defaultVal;
                }
            };            


            dir.isAttrEmpty = function(attrVal) {
                if(attrVal && attrVal.trim() !== "") {
                    return false;
                }
                return true;
            };

            dir.imageOnLoad = function() {
                dir.updateFallback(true);
                scope.$apply(); //need to apply because this is not an Angular event
            };

            dir.imageOnError = function() {
                // logc("Error loading image: %s\n\t >> replacing with default", dir.imgElem.attr("src"));
                dir.updateFallback(false);
                scope.$apply(); //need to apply because this is not an Angular event          
            };


            dir.updateFallback = function(flag) {
                scope.isSrcValid = flag;
                dir.setImageStyles(!flag);
            };

            dir.setImageStyles = function(useDefault) {
                if(useDefault) {
                    dir.setHeight(scope.height, scope.imageSize || defaultStyles.size);
                    dir.setWidth(scope.width, scope.imageSize || defaultStyles.size);
                    dir.setIconSize(scope.iconSize, scope.imageSize);
                    dir.setBorderRadius(scope.radius, defaultStyles.borderRadius);
                } else { //if image is valid, use whatever has been set only
                    dir.setHeight(scope.height, scope.imageSize);
                    dir.setWidth(scope.width, scope.imageSize);
                    dir.setBorderRadius(scope.radius);
                }
            };

            dir.init();        

        }

        return {
            link: link,
            restrict: "E",
            replace: true,  
            scope: {
                imageSrc: "<", //set the user image source
                imageSize: "<", //sets the height, width, and iconSize; default: 50px

                containerRadius: "<radius", //sets the border-radius of the gray area; default: 50%
                iconSize: "<", //sets the size of the user icon; default: imageSize, width
                height: "<", //sets the height of the gray area; default: imageSize
                width: "<" //sets the width of the gray area; default imageSize                
            },
            templateUrl: "common/user-image/templates/image-wrapper.html"
        };
    }

    angular
        .module("settings")
        .directive("rpUserImage", [
            userImageDirective
        ]);
})(angular);
