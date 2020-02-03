//  AData Model

(function(angular, undefined) {
    "use strict";

    function factory(switchModel, $filter) {
        function AData() {
            var s = this;
            s.init();
        }

        var p = AData.prototype;

        p.init = function() {
            var s = this;

            s.changed = false;
            s.active = false;
            s.data = {
                productId: 8, //TODO: Enum api for products instead of hard coded
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    companiesList: [],
                    hasAccessToSiteSpendManagementOnly: false,
                    hasAccessToAllCurrentFutureProperties: false,
                    isAccountingAdmin: false                    
                }
            };

            s.roles = [];
            s.properties = [];
            s.companies = [];
            s.entities = [];
            s._data = angular.copy(s.data);
        };

        p.setChanged = function() {
            var s = this;
            s.changed = true;
            return s;
        };

        p.hasChanged = function() {
            var s = this;
            return s.changed;
        };

        p.setActive = function(bool) {
            var s = this;
            s.active = bool;
            return s;
        };

        p.isActive = function() {
            var s = this;
            return s.active;
        };

        p.setAccessToSiteSpendMgmtOnly = function(bool) {
            var s = this;
            s.hasAccessToSiteSpendManagementOnly = bool;
            return s;
        };

        p.setMConsole = function(bool) {
            var s = this;
            s.isMConsole = bool;
            console.log("s.isMConsole", s.isMConsole);
            return s;
        };

        p.setAllowAccessToCurrentFutureProp = function(bool) {
            var s = this;
            s.hasAccessToAllCurrentFutureProperties = bool;
            return s;
        };

        p.setAccountingAdmin = function(bool) {
            var s = this;
            s.isAccountingAdmin = bool;
            return s;
        };


        p.setCompanies = function(companiesData) {
            var s = this;
            s.companies = companiesData;
        };

        p.getCompanies = function() {
            var s = this;
            return s.companies;
        };

        p.setProperties = function(propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.getProperties = function() {
            var s = this;
            return s.properties;
        };

        p.setRoles = function(rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.setallCompanies = function(companiesData, val) {
            var s = this;

            companiesData.forEach(function (item) {
               item["isAssigned"] = val;
            });

            //s.companies = companiesData;
        };

        p.setAllEntities = function (EntitiesData,val) {
            var s = this;

            EntitiesData.forEach(function (item) {
               item["isAssigned"] = val;
            });

            //s.entities = EntitiesData;
        };

        p.setAllProperties = function(propertiesData, val) {
            var s = this;

            propertiesData.forEach(function (item) {
               item["isAssigned"] = val;
            });

            s.properties = propertiesData;
        };

        p.setAllRoles = function(rolesData, val) {
            var s = this;

            rolesData.forEach(function (item) {
               item["isAssigned"] = val;
            });

            s.roles = rolesData;
        };

        p.getRoles = function() {
            var s = this;
            return s.roles;
        };


        p.getData = function() {
            var s = this,
                hasRoles = false,
                hasProperties = false,
                hasCompanies = false;

            s.data.inputJson.isAccountingAdmin = switchModel.getIsAccountingAdmin();
            s.data.inputJson.hasAccessToSiteSpendManagementOnly = switchModel.getIsAccessToSiteSpendMgmtOnly();
            s.data.inputJson.hasAccessToAllCurrentFutureProperties = switchModel.getAllProperties();

             
            if (s.data.inputJson.hasAccessToAllCurrentFutureProperties && !s.data.inputJson.isAccountingAdmin) {
                s.properties = [];
                s.properties.push("all");

                s.companies = [];
                s.companies.push("all");
            }

            if (s.data.inputJson.isAccountingAdmin && !s.data.inputJson.hasAccessToAllCurrentFutureProperties) {
                // s.roles = [];
                // s.roles.push("all");
                
                // Set the Entities to ALL
                s.properties = [];
                s.properties.push("all");
            }

            if (s.data.inputJson.hasAccessToAllCurrentFutureProperties && s.data.inputJson.isAccountingAdmin) {
                s.properties = [];
                s.properties.push("all");

                s.companies = [];
                s.companies.push("all");

                // s.roles = [];
                // s.roles.push("all");
            }

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                if (s.roles[0] !== "all") {
                    s.roles.forEach(function(role) {
                        if (role.isAssigned) {
                            s.data.inputJson.roleList.push(role.id);
                        }
                    });
                } else {
                    s.data.inputJson.roleList.push("all");
                }

                hasRoles = s.data.inputJson.roleList.length > 0;
            }

            if (s.properties && s.properties.length) {
                s.data.inputJson.propertyList = [];

                if (s.properties[0] !== "all") {

                    s.companies.forEach(function(comp) {

                        if (s.properties[0].mConsoleId !== "") {

                            if (comp.isAssigned) {
                                s.data.inputJson.propertyList.push(comp.id);
                            }
                             // else {
                                s.properties.forEach(function(prop) {

                                    if (prop.companyId === comp.id) {
                                        if (prop.isAssigned) {
                                            if(prop.propertyId !== "")
                                            {
                                                s.data.inputJson.propertyList.push(comp.id + "|" + prop.propertyId);
                                            }else{
                                                s.data.inputJson.propertyList.push(comp.id );
                                            }
                                        }
                                    }
                                });

                            // }

                        } else {

                            if (comp.isAssigned) {
                                s.data.inputJson.propertyList.push(comp.id);
                            }

                            s.properties.forEach(function(prop) {

                                if (prop.companyId === comp.id) {
                                    if (prop.isAssigned) {
                                        s.data.inputJson.propertyList.push(prop.propertyId);
                                    }
                                }
                            });
                        }
                    });
                   
                } else {
                    s.data.inputJson.propertyList.push("all");
                }

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            if (s.companies && s.companies.length) {
                s.data.inputJson.companiesList = [];

                if (s.companies[0] !== "all") {
                    s.companies.forEach(function(comp) {
                        if (comp.isAssigned) {
                            s.data.inputJson.companiesList.push(comp.id);
                        }
                    });
                } else {
                    s.data.inputJson.companiesList.push("all");
                }

                hasCompanies = s.data.inputJson.companiesList.length > 0;
            }



            var isCompSelwithProperties = true; // if prop is selected but comp is NOT selected then set to false
            if(!s.data.inputJson.isMConsole){  
                s.companies.forEach(function(comp) {

                    if (comp.isAssigned === false) {

                            s.properties.forEach(function(prop) {

                                if (prop.companyId === comp.id) {
                                    if (prop.isAssigned) {
                                        isCompSelwithProperties = false;
                                        return;
                                    }
                                }
                            });

                    }
                });
            }

            if(!s.isMConsole){  
                if ( hasRoles && hasProperties) { 
                    return s.data;
                }
            }else{
                if (hasCompanies && hasRoles && hasProperties && isCompSelwithProperties) { // No need to check hasCompanies - not mandatory
                    return s.data;
                }
             }

            return null;
        };

        p.clearProperties = function() {
            var s = this;
            // set everything to false
            if (s.getProperties() != undefined) {
                if (s.getProperties()[0] !== "all") {
                    s.getProperties().forEach(function(item) {
                        item.disableSelection = false;
                        item.isAssigned = false;
                    });
                }
            }

            if (s.getCompanies() != undefined) {
                if (s.getCompanies()[0] !== "all") {
                    s.getCompanies().forEach(function(item) {
                        item.isAssigned = false;
                    });
                }
            }
        };

        p.clearRoles = function() {
            var s = this;
            // set everything to false
            if (s.getRoles() != undefined) {

                if (s.getRoles()[0] !== "all") {
                    var roles = $filter("filter")(s.getRoles(), {
                        isAssigned: true
                    });

                    roles.forEach(function(item) {
                        item.isAssigned = false;
                    });
                }
            }
        };

        p.reset = function() {
            var s = this;

            s.roles = [];
            s.active = false;
            s.changed = false;
            s.properties = [];
            s.companies = [];
            s.entities = [];

            s.data = angular.copy(s._data);
        };

        return new AData();
    }

    angular
        .module("settings")
        .factory("AccountingDataModel", ["ASwitchModel", "$filter", factory]);
})(angular);
