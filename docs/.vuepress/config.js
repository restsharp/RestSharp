module.exports = {
    title: "RestSharp",
    description: "Simple .NET client for HTTP REST APIs",
    plugins: ["@vuepress/active-header-links"],
    head: [
        ["link", {rel: "apple-touch-icon", sizes: "180x180", href: "/apple-touch-icon.png"}],
        ["link", {rel: "icon", type: "image/png", sizes: "32x32", href: "/favicon-32x32.png"}],
        ["link", {rel: "icon", type: "image/png", sizes: "16x16", href: "/favicon-16x16.png"}],
        ["link", {rel: "manifest", href: "/site.webmanifest"}]
    ],
    themeConfig: {
        logo: "/restsharp.png",
        navbar: [
            {text: "Migration from legacy", link: "/v107/"},
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
                    header: "Migration from legacy",
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
