let rolesApiRequirements = [
    {
        name: "OneSite",
        apiName : "onesite",
        roles: [
            {
                method: "GET",
                queriesString: '/role?',
                queries: [
                    "editorPersonaId"
                ]
            },
            {
                method: "POST",
                queriesString: '/role?',
                queries: [
                    "editorPersonaId",
                    "roleName"
                ]
            },
            {
                method: "PUT",
                queriesString: '/role/rights?',
                queries: [
                    "editorPersonaId",
                    "roleId"
                ]
            },
        ],
        rights: [
            {
                method: "GET",
                queriesString: '/rights?',
                queries: [
                    "editorPersonaId",
                ]
            }
        ]
    },
    {
        name: "Unified Login",
        apiName : "unifiedlogin",
            roles: [
                {
                    method: "GET",
                    queriesString: '/rolesCount?',
                    queries: [
                        "editorPersonaId",
                        "partyId",
                        "userPersonaId",
                    ]
                },
                {
                    method: "POST",
                    queriesString: '/role?',
                    queries: [
                        "editorPersonaId",
                        "partyId",
                        "roleName",
                    ]
                },
                {
                    method: "PUT",
                    queriesString: '/role/rights?',
                    queries: [
                        "editorPersonaId",
                        "roleId",
                    ]
                }
            ],
            rights: [
                {
                    method: "GET",
                    queriesString: '/rightsCount?',
                    queries: [
                        "editorPersonaId",
                        "partyId",
                    ]
                }
            ]
    }
];

