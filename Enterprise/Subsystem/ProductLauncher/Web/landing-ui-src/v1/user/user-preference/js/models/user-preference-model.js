(function (angular, undefined) {
  "use strict";

  function factory(_, formConfig) {
    function UserPreferenceModel() {
      var s = this;
      s.init();
    }

    var p = UserPreferenceModel.prototype;

    p.init = function () {
      var s = this;
      s.data = {};
    };

    p.getData = function () {
      var s = this;
      s.data.categories = s.getCategories();
      return s.data;
    };

    p.bindCategories = function (categories, products) {
      var s = this;
      var productInfo = products.products.concat(products.resources);
      var result = categories.filter(function(n) {
        return productInfo.find(function(item){ return item.productCode === n.productCode;});
      });
      s.data.masterData = result;
      var uniqueData = _.uniq(result, "productTileId");
      var productsData = [];
      uniqueData.forEach(function (element, index) {
        var obj = {};
        obj.title = element.productTileName;
        obj.name = element.productTileId;
        obj.isSelected = index === 0 ? true : false;
        productsData.push(obj);
      });
      s.data.products = [];
      s.data.products.push({
        title: "Notifications",
        name: "0",
        items: productsData,
      });

      s.getTypeListData(productsData.length > 0 ? productsData[0].name : "");
      return s;
    };

    p.updatePreferenceList = function (resp) {
      var s = this;
      resp.preferences.forEach(function (item) {
        s.data.userPreferencesList = [];
        s.data.userPreferencesList.push({
          text: item.name,
          value: item.id.toString(),
        });
      });
      return s;
    };

    p.getTypeListData = function (name) {
      var s = this;
      if (s.data.typeData && s.data.typeData.length > 0) {
        s.data.typeData.forEach(function (typeData) {
            typeData.catArray.forEach(function (item) {
            var index = s.data.masterData.findIndex(function (masterdata) {
              return masterdata.id === item.id;
            });
            if(index>-1){
            s.data.masterData[index].userPreferences.forEach(function (pref) {
            pref.isEnabled =
                pref.preferenceId === 1 ? item.preferenceInfo === 2 || item.preferenceInfo === 1
                : pref.preferenceId === 2?  item.preferenceInfo === 2
                : false;
            });
           // s.data.masterData[index] = angular.copy(item);
            }
          });
        });
      }
      s.data.typeData = [];
      var selectedProductInfo = s.data.masterData.filter(function (item) {
        return item.productTileId === parseInt(name);
      });
      s.data.selectedProductName =
        selectedProductInfo.length > 0  ? selectedProductInfo[0].productTileName
          : "";
      var ProductConfiguration = _.groupBy(selectedProductInfo, "productId");
      var keys = Object.keys(ProductConfiguration);
      keys.forEach(function (key) {
        if (ProductConfiguration[key].length > 0) {
          s.data.typeData.push({ productName: key, catArray: [] });
          ProductConfiguration[key].forEach(function (item) {
            var data = [
              { value: 0, text: "None" },
              { value: 1, text: "Feed" },
              { value: 2, text: "Feed & Banner" },
            ];
            formConfig.setOptions("preferenceConfig", data);
            s.data.typeData[s.data.typeData.length - 1].productName =
              item.productName;
            s.data.typeData[s.data.typeData.length - 1].catArray.push({
              categoryNameId: item.categoryNameId,
              categoryName: item.name,
              id: item.id,
              preferenceId: [],
              userPreferencesList: angular.copy(s.data.userPreferencesList),
              userPreferences: item.userPreferences,
              config: formConfig,
              preferenceInfo: 0,
            });
            item.userPreferences.forEach(function (element) {
              var index =
                s.data.typeData[s.data.typeData.length - 1].catArray.length - 1;
              if (element.isEnabled === true) {
                s.data.typeData[s.data.typeData.length - 1].catArray[
                  index
                ].preferenceId.push(element.preferenceId.toString());
              }
              s.data.typeData[s.data.typeData.length - 1].catArray[
                index
              ].preferenceInfo =
                s.data.typeData[s.data.typeData.length - 1].catArray[index]
                  .preferenceId.length > 1 ? 2
                  : s.data.typeData[s.data.typeData.length - 1].catArray[index]
                      .preferenceId > 0  ? 1
                  : 0;
            });
          });
        }
      });
      return s;
    };

    p.reset = function () {
      var s = this;
      s.data = {};
      s.telecomNumbers.forEach(s.destroyItem);
      s.telecomNumbers = [];
      s.electronicEmails.forEach(s.destroyItem);
      s.electronicEmails = [];
    };

    return new UserPreferenceModel();
  }

  angular
    .module("settings")
    .factory("UserPreferenceModel", [
      "underscore",
      "UserPreferenceFormConfig",
      factory,
    ]);
})(angular);
