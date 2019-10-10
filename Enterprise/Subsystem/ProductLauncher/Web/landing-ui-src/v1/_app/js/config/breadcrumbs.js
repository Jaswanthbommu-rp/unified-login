//  Configure Breadcrumbs

(function (angular) {
    "use strict";

    function config(prov) {
        var links = {
            home: {
                href: "/home/",
                text: "RealPage"
            },

            products: {
                href: "#/products",
                text: "Products"
            },

            people: {
                href: "/home/",
                text: "Unified Platform"
            },

            users: {
                href: "/home/people/users",
                text: "Users"
            }
        };

        var breadcrumbs = [
            {
                url: /^\/$/,
                appHeader: {
                    text: "Home"
                },
                productName: "Home",
                links: [],
                activePage: {
                    text: "Home"
                }
            },
            {
                url: /^\/products/,
                appHeader: {
                    text: "Products"
                },
                productName: "Products",
                links: [
                ],
                activePage: {
                    text: "Products"
                }
            },
            {
                url: /^\/product\/lrc/,
                appHeader: {
                    text: "Leasing & Rents"
                },
                links: [
                    links.products
                ],
                activePage: {
                    text: "Leasing & Rents"
                }
            },

            //PEOPLE

            {
                url: /^\/user\/add$/,
                appHeader: {
                    text: "Add User"
                },
                productName: "People",
                links: [
                    links.people,
                    links.users
                ],
                activePage: {
                    text: "Add User"
                }
            },

            {
                url: /^\/user\/[a-z0-9\-]+\/edit$/,
                appHeader: {
                    text: "Edit User"
                },
                productName: "People",
                links: [
                    links.people,
                    links.users
                ],
                activePage: {
                    text: "Edit User"
                }
            },

            {
                url: /^\/user\/[a-z0-9\-]+\/UserList\/edit$/i,
                appHeader: {
                    text: "Edit User"
                },
                productName: "People",
                links: [
                    links.people,
                    links.users
                ],
                activePage: {
                    text: "Edit User"
                }
            },

            {
                url: /^\/user\/[a-z0-9\-]+\/ManageProfile\/edit$/i,
                appHeader: {
                    text: "Edit User"
                },
                productName: "People",
                links: [
                    links.people,
                    links.users
                ],
                activePage: {
                    text: "Edit User"
                }
            },

            {
                url: /^\/user\/[a-z0-9\-]+\/clone$/,
                productName: "People",
                appHeader: {
                    text: "Clone User"
                },
                links: [
                    links.people,
                    links.users
                ],
                activePage: {
                    text: "Clone User"
                }
            },

            {
                url: /^\/people\/user\/add/,
                appHeader: {
                    text: "Add User"
                },
                productName: "People",
                links: [
                    links.people,
                    links.users
                ],
                activePage: {
                    text: "Add User"
                }
            },
            {
                url: /^\/people\/user\/[\w-]+/,
                appHeader: {
                    text: "User Details"
                },
                productName: "People",
                links: [
                    links.people,
                    links.users
                ],
                activePage: {
                    text: "User Details"
                }
            },
            {
                url: /^\/people\/users/,
                appHeader: {
                    text: "Users"
                },
                productName: "People",
                links: [
                    links.people
                ],
                activePage: {
                    text: "Users"
                }
            },
            {
                url: /^\/people\/activity-log/,
                appHeader: {
                    text: "User Activity Log"
                },
                productName: "People",
                links: [
                    links.people
                ],
                activePage: {
                    text: "User Activity Log"
                }
            },
            {
                url: /^\/roles-and-rights/,
                appHeader: {
                    text: "Roles & Rights"
                },
                productName: "Roles & Rights",
                links: [
                ],
                activePage: {
                    text: "Roles & Rights"
                }
            },
            {
                url: /^\/employee-access/,
                appHeader: {
                    text: "Support"
                },
                productName: "Support Tool",
                links: [
                ],
                activePage: {
                    text: "Support Tool"
                }
            }
        ];

        prov.setLinks(links).setBreadcrumbs(breadcrumbs);
    }

    angular
        .module("settings")
        .config(["rpBreadcrumbsModelProvider", config]);
})(angular);
