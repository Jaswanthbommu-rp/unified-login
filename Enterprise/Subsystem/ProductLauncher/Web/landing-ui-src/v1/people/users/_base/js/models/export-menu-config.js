//  Export Menu Config

(function (angular, undefined) {
    "use strict";

    function factory(appLangTranslate, exportMenuConfig, PAPER_SIZE, EXPORT_TYPE) {
        var langText = appLangTranslate("people.user-list").translate;

        return function (src) {
            return exportMenuConfig({
                menuOffsetLeft: -50,
                dataRetriever: src.getExportData,
                menuClassNames: "user-list-export-menu-panel",
                menuItems: [
                    {
                        context: {
                            exportBit: true,
                            fileType: EXPORT_TYPE.CSV,
                            paperSize: PAPER_SIZE.LEGAL,
                            fileName: langText("export.fileName")
                        },
                        text: langText("text.exportToCSV")
                    },
                    {
                        context: {
                            exportBit: true,
                            fileType: EXPORT_TYPE.PDF,
                            paperSize: PAPER_SIZE.LEGAL,
                            fileName: langText("export.fileName")
                        },
                        text: langText("text.exportToPDF")
                    }
                ]
            });
        };
    }

    angular
        .module("settings")
        .factory("userListExportMenuConfig", [
            "appLangTranslate",
            "rpExportMenuConfig",
            "PAPER_SIZE",
            "EXPORT_REPORT_TYPE",
            factory
        ]);
})(angular);
