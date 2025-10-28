// @ts-check
// `@type` JSDoc annotations allow editor autocompletion and type checking
// (when paired with `@ts-check`).
// There are various equivalent ways to declare your Docusaurus config.
// See: https://docusaurus.io/docs/api/docusaurus-config

import {themes as prismThemes} from 'prism-react-renderer';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'HiveMQtt Documentation',
  tagline: 'The Spectacular (BETA) C# MQTT Client for .NET',
  favicon: 'img/favicon.ico',

  // Set the production url of your site here
  url: 'https://your-docusaurus-site.example.com',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/hivemq-mqtt-client-dotnet',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'hivemq', // Usually your GitHub org/user name.
  projectName: 'hivemq-mqtt-client-dotnet', // Usually your repo name.
  deploymentBranch: 'gh-pages',
  trailingSlash: false,

  onBrokenLinks: 'warn',

  markdown: {
    format: 'detect',
    hooks: {
      onBrokenMarkdownLinks: 'warn'
    }
  },

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: './sidebars.js',
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            'https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Documentation',
        },
        theme: {
          customCss: './src/css/custom.css',
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      // Replace with your project's social card
      image: 'img/docusaurus-social-card.jpg',
      navbar: {
  	    title: 'HiveMQtt: A C# MQTT Client by HiveMQ',
        logo: {
          alt: 'HiveMQ Logo',
          src: 'img/logo.png',
        },
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'docsSidebar',
            position: 'left',
            label: 'Docs',
          },
          {to: '/docs/events', label: 'Events', position: 'left'},
          {to: '/docs/category/how-to-guides', label: 'How-To\'s', position: 'left'},
          {
            href: 'https://github.com/hivemq/hivemq-mqtt-client-dotnet',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Sections',
            items: [
              {
                label: 'Docs',
                to: '/docs/intro',
              },
              {
                label: 'Events',
                to: '/docs/events',
              },
              {
                label: 'How-To\'s',
                to: '/docs/category/how-to-guides',
              },
            ],
          },
          {
            title: 'Community',
            items: [
              {
                label: 'HiveMQ Community Forum',
                href: 'https://community.hivemq.com/',
              },
              {
                label: 'Stack Overflow',
                href: 'https://stackoverflow.com/questions/tagged/hivemq',
              },
              {
                label: 'Twitter',
                href: 'https://twitter.com/hivemq',
              },
            ],
          },
          {
            title: 'More',
            items: [
              {
                label: 'HiveMQ',
                href: 'https://www.hivemq.com/',
              },
              {
                label: 'HiveMQ Cloud',
                href: 'https://www.hivemq.com/cloud/',
              },
              {
                label: 'HiveMQ Cloud Console',
                href: 'https://console.hivemq.cloud/',
              },
              {
                label: 'GitHub',
                href: 'https://github.com/hivemq/hivemq-mqtt-client-dotnet',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} HiveMQ, GmbH.`,
      },
      prism: {
        additionalLanguages: ['csharp'],
        theme: prismThemes.github,
        darkTheme: prismThemes.dracula,
      },
    }),
};

export default config;
