//  Export Menu Config

(function (angular, undefined) {
    "use strict";

    function factory(exportMenuConfig, PAPER_SIZE, EXPORT_TYPE) {

        return function (src) {
            return exportMenuConfig({
                menuOffsetLeft: -130,
                dataRetriever: src.getExportData,
                menuClassNames: "activity-log-aside-export-menu-panel",
                menuItems: [
                    {
                        context: {
                            exportBit: true,
                            fileType: EXPORT_TYPE.CSV,
                            paperSize: PAPER_SIZE.LEGAL,
                            fileName: "activitylog"
                        },
                        text: "Export to CSV"
                    },
                    {
                        context: {
                            exportBit: true,
                            fileType: EXPORT_TYPE.PDF,
                            paperSize: PAPER_SIZE.LEGAL,
                            fileName: "activitylog"
                        },
                        text: "Export to PDF"
                    }
                ]
            });
        };
    }

    angular
        .module("settings")
        .factory("activityLogAsideExportMenuConfig", [
            "rpExportMenuConfig",
            "PAPER_SIZE",
            "EXPORT_REPORT_TYPE",
            factory
        ]);
})(angular);
