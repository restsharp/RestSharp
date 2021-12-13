module.exports = {
    title: "RestSharp",
    description: "Simple .NET client for HTTP REST APIs",
    plugins: ["@vuepress/active-header-links"],
    themeConfig: {
        logo: "/restsharp.png",
        navbar: [
            {text: "vNext", link: "/v107/"},
            {text: "Documentation", link: "/intro.html"},
            {text: "Get help", link: "/support/"},
            {text: "NuGet", link: "https://nuget.org/packages/RestSharp"}
        ],
        sidebarDepth: 2,
        sidebar: {
            "/": [
                {
                    text: "",
                    header: "RestSharp",
                    children: [
                        "intro.md",
                        "usage.md",
                        "authenticators.md"
                    ]
                }
            ],
            "/v107/": [
                {
                    text: "",
                    header: "RestSharp vNext",
                    children: [
                        "/v107/README.md"
                    ]
                }
            ],
            "/support/": [
                {
                    text: "",
                    header: "Get help",
                    children: [
                        "/support/README.md"
                    ]
                }
            ]
        },
        //     [
        //     {
        //         title: "Getting Started",
        //         path: "/getting-started/",
        //         collapsable: false,
        //         children: [
        //             "/getting-started/",
        //             "/getting-started/getting-started"
        //         ]
        //     },
        //     {
        //         title: "Using RestSharp",
        //         path: "/usage/",
        //         collapsable: false,
        //         children: [
        //             "/usage/serialization",
        //             "/usage/files",
        //             "/usage/authenticators",
        //             "/usage/parameters",
        //             "/usage/exceptions"
        //         ]
        //     },
        //     {
        //         title: "Got stuck?",
        //         path: "/get-help/",
        //         collapsable: false,
        //         children: [
        //             "/get-help/faq"
        //         ]
        //     }
        // ],
        searchPlaceholder: "Search...",
        lastUpdated: "Last Updated",
        repo: "restsharp/RestSharp",

        docsRepo: "restsharp/RestSharp",
        docsDir: "docs",
        docsBranch: "dev",
        editLinks: true,
        editLinkText: "Help us by improving this page!"
    }
}
