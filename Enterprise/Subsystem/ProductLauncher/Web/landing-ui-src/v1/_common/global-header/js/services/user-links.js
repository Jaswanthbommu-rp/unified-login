//  Global Header User Links

(function (angular, undefined) {
    "use strict";

    function GlobalHeaderUserLinks(headerModel, dashboardModel, pubsub, baseFormConfig, modalCompSwitch, persona, manageProfileSvc, clientPortalSvc, switchCompanySvc) {
        var svc = this;
         svc.model = baseFormConfig();

        svc.unsub = angular.noop;
        svc.links = [];
        svc.init = function () {
           
            svc.unsub();
            svc.unsub = dashboardModel.subscribe(svc.setLinks);
            svc.personaWatch = angular.noop;
            svc.companySwitchWatch = angular.noop;
            svc.model.setMethodsSrc(svc);
        };

        svc.setLinks = function (resp) {
            var resData = resp.dashboardElements.resources,
                clientPortalLink = svc.getClientPortalLink(resData),
                manageProfileActive = true;
                                               
           
            if (manageProfileActive) {
                var url = "#/user/" + resp.dashboardElements.profileDetail.userLogin.realPageId + "/ManageProfile" + "/edit";
                logc(url);
                manageProfileSvc.activate().setLink(url);
            }

            if (clientPortalLink) {                
                clientPortalSvc.activate().setLink(clientPortalLink.url);
            }

            if (persona.isReady()) {                
                svc.setCompanySwitchLink();
            } else {                            
                svc.personaWatch = persona.subscribe(svc.setCompanySwitchLink);
            }
            
        };

        svc.getClientPortalLink = function (data) {
            var clientPortalLink;

            (data || []).forEach(function (item) {
                if (item.productId === 14) {
                    clientPortalLink = {
                        slot: "",
                        name: "clientPortal",
                        text: "Client Portal",
                        url: item.productUrl,
                        target: "_cp"
                    };
                }
            });

            return clientPortalLink;
        };
        
        svc.setCompanySwitchLink = function () {      
            if (persona.data.hasMultiCompany) { 
                pubsub.subscribe("raulSwitchCompany.show", svc.openModal);   

                switchCompanySvc.activate();
            }
        };

        
        svc.openModal = function ($event) {                        
            modalCompSwitch.show();                        
        };

        
    }

    angular
        .module("settings")
        .service("globalHeaderUserLinks", [
            "rpGlobalHeaderModel",
            "dashboardModel",
            "pubsub",            
            "baseFormConfig",
            "compSwitchModal",
            "personaDetails",
            "raulManageProfileSvc",
            "raulClientPortalSvc",
            "raulSwitchCompanySvc",
            GlobalHeaderUserLinks
        ]);
})(angular);
