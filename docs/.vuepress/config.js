module.exports = {
    title: "RestSharp",
    description: "Simple .NET client for HTTP REST APIs",
    plugins: ["@vuepress/active-header-links"],
    themeConfig: {
        logo: "/restsharp.png",
        navbar: [
            {text: "Migration to v107", link: "/v107/"},
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
                        "serialization.md",
                        "authenticators.md",
                        "error-handling.md"
                    ]
                }
            ],
            "/v107/": [
                {
                    text: "",
                    header: "Migration to v107",
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
