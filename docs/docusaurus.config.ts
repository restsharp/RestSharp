import type {Config} from "@docusaurus/types";
import type * as Preset from "@docusaurus/preset-classic";
import {themes} from "prism-react-renderer";

const config: Config = {
  title: "RestSharp",
  tagline: "Simple REST and HTTP API Client for .NET",
  favicon: "img/favicon.ico",
  url: "https://restsharp.dev",
  baseUrl: "/",
  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "warn",
  i18n: {
    defaultLocale: "en",
    locales: ["en"],
  },

  presets: [
    [
      "classic",
      {
        docs: {
          sidebarPath: "./sidebars.ts",
        },
        theme: {
          customCss: "./src/css/custom.css",
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    navbar: {
      title: "RestSharp",
      logo: {
        alt: "RestSharp Logo",
        src: "img/restsharp.png",
      },
      items: [
        {
          type: "docSidebar",
          sidebarId: "tutorialSidebar",
          position: "left",
          label: "Documentation",
        },
        {
          href: "/support",
          label: "Support",
        },
        {
          href: 'https://github.com/RestSharp/RestSharp',
          label: "GitHub",
          position: "right",
        },
      ],
    },
    footer: {
      style: "dark",
      links: [
        {
          title: "Docs",
          items: [
            {
              label: "Documentation",
              to: "/docs/intro",
            },
          ],
        },
        {
          title: "Community",
          items: [
            {
              label: "Stack Overflow",
              href: "https://stackoverflow.com/questions/tagged/restsharp",
            },
            {
              label: "Discord",
              href: "https://discordapp.com/invite/docusaurus",
            },
            {
              label: "Twitter",
              href: "https://twitter.com/RestSharp",
            },
          ],
        },
        {
          title: "More",
          items: [
            {
              label: "GitHub",
              href: "https://github.com/RestSharp/RestSharp",
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} .NET Foundation. Built with Docusaurus.`,
    },
    prism: {
      theme: themes.vsLight,
      darkTheme: themes.vsDark,
      additionalLanguages: ['csharp'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
