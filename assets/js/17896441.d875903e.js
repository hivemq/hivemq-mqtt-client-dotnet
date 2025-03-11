"use strict";(self.webpackChunkdocumentation=self.webpackChunkdocumentation||[]).push([[8401],{1243:(e,t,s)=>{s.d(t,{A:()=>v});s(6540);var a=s(8215),n=s(7559),i=s(1754),l=s(9169),o=s(8774),r=s(1312),d=s(6025),c=s(4848);function u(e){return(0,c.jsx)("svg",{viewBox:"0 0 24 24",...e,children:(0,c.jsx)("path",{d:"M10 19v-5h4v5c0 .55.45 1 1 1h3c.55 0 1-.45 1-1v-7h1.7c.46 0 .68-.57.33-.87L12.67 3.6c-.38-.34-.96-.34-1.34 0l-8.36 7.53c-.34.3-.13.87.33.87H5v7c0 .55.45 1 1 1h3c.55 0 1-.45 1-1z",fill:"currentColor"})})}const m={breadcrumbHomeIcon:"breadcrumbHomeIcon_YNFT"};function h(){const e=(0,d.A)("/");return(0,c.jsx)("li",{className:"breadcrumbs__item",children:(0,c.jsx)(o.A,{"aria-label":(0,r.T)({id:"theme.docs.breadcrumbs.home",message:"Home page",description:"The ARIA label for the home page in the breadcrumbs"}),className:"breadcrumbs__link",href:e,children:(0,c.jsx)(u,{className:m.breadcrumbHomeIcon})})})}const b={breadcrumbsContainer:"breadcrumbsContainer_Z_bl"};function p(e){let{children:t,href:s,isLast:a}=e;const n="breadcrumbs__link";return a?(0,c.jsx)("span",{className:n,itemProp:"name",children:t}):s?(0,c.jsx)(o.A,{className:n,href:s,itemProp:"item",children:(0,c.jsx)("span",{itemProp:"name",children:t})}):(0,c.jsx)("span",{className:n,children:t})}function x(e){let{children:t,active:s,index:n,addMicrodata:i}=e;return(0,c.jsxs)("li",{...i&&{itemScope:!0,itemProp:"itemListElement",itemType:"https://schema.org/ListItem"},className:(0,a.A)("breadcrumbs__item",{"breadcrumbs__item--active":s}),children:[t,(0,c.jsx)("meta",{itemProp:"position",content:String(n+1)})]})}function v(){const e=(0,i.OF)(),t=(0,l.Dt)();return e?(0,c.jsx)("nav",{className:(0,a.A)(n.G.docs.docBreadcrumbs,b.breadcrumbsContainer),"aria-label":(0,r.T)({id:"theme.docs.breadcrumbs.navAriaLabel",message:"Breadcrumbs",description:"The ARIA label for the breadcrumbs"}),children:(0,c.jsxs)("ul",{className:"breadcrumbs",itemScope:!0,itemType:"https://schema.org/BreadcrumbList",children:[t&&(0,c.jsx)(h,{}),e.map(((t,s)=>{const a=s===e.length-1,n="category"===t.type&&t.linkUnlisted?void 0:t.href;return(0,c.jsx)(x,{active:a,index:s,addMicrodata:!!n,children:(0,c.jsx)(p,{href:n,isLast:a,children:t.label})},s)}))]})}):null}},3761:(e,t,s)=>{s.r(t),s.d(t,{default:()=>$});var a=s(6540),n=s(1003),i=s(9532),l=s(4848);const o=a.createContext(null);function r(e){let{children:t,content:s}=e;const n=function(e){return(0,a.useMemo)((()=>({metadata:e.metadata,frontMatter:e.frontMatter,assets:e.assets,contentTitle:e.contentTitle,toc:e.toc})),[e])}(s);return(0,l.jsx)(o.Provider,{value:n,children:t})}function d(){const e=(0,a.useContext)(o);if(null===e)throw new i.dV("DocProvider");return e}function c(){const{metadata:e,frontMatter:t,assets:s}=d();return(0,l.jsx)(n.be,{title:e.title,description:e.description,keywords:t.keywords,image:s.image??t.image})}var u=s(8215),m=s(4581),h=s(6929);function b(){const{metadata:e}=d();return(0,l.jsx)(h.A,{previous:e.previous,next:e.next})}var p=s(1878),x=s(4267),v=s(7559),j=s(1312);function g(e){let{lastUpdatedAt:t,formattedLastUpdatedAt:s}=e;return(0,l.jsx)(j.A,{id:"theme.lastUpdated.atDate",description:"The words used to describe on which date a page has been last updated",values:{date:(0,l.jsx)("b",{children:(0,l.jsx)("time",{dateTime:new Date(1e3*t).toISOString(),children:s})})},children:" on {date}"})}function f(e){let{lastUpdatedBy:t}=e;return(0,l.jsx)(j.A,{id:"theme.lastUpdated.byUser",description:"The words used to describe by who the page has been last updated",values:{user:(0,l.jsx)("b",{children:t})},children:" by {user}"})}function A(e){let{lastUpdatedAt:t,formattedLastUpdatedAt:s,lastUpdatedBy:a}=e;return(0,l.jsxs)("span",{className:v.G.common.lastUpdated,children:[(0,l.jsx)(j.A,{id:"theme.lastUpdated.lastUpdatedAtBy",description:"The sentence used to display when a page has been last updated, and by who",values:{atDate:t&&s?(0,l.jsx)(g,{lastUpdatedAt:t,formattedLastUpdatedAt:s}):"",byUser:a?(0,l.jsx)(f,{lastUpdatedBy:a}):""},children:"Last updated{atDate}{byUser}"}),!1]})}var _=s(8774);const N={iconEdit:"iconEdit_Z9Sw"};function L(e){let{className:t,...s}=e;return(0,l.jsx)("svg",{fill:"currentColor",height:"20",width:"20",viewBox:"0 0 40 40",className:(0,u.A)(N.iconEdit,t),"aria-hidden":"true",...s,children:(0,l.jsx)("g",{children:(0,l.jsx)("path",{d:"m34.5 11.7l-3 3.1-6.3-6.3 3.1-3q0.5-0.5 1.2-0.5t1.1 0.5l3.9 3.9q0.5 0.4 0.5 1.1t-0.5 1.2z m-29.5 17.1l18.4-18.5 6.3 6.3-18.4 18.4h-6.3v-6.2z"})})})}function C(e){let{editUrl:t}=e;return(0,l.jsxs)(_.A,{to:t,className:v.G.common.editThisPage,children:[(0,l.jsx)(L,{}),(0,l.jsx)(j.A,{id:"theme.common.editThisPage",description:"The link label to edit the current page",children:"Edit this page"})]})}const T={tag:"tag_zVej",tagRegular:"tagRegular_sFm0",tagWithCount:"tagWithCount_h2kH"};function U(e){let{permalink:t,label:s,count:a}=e;return(0,l.jsxs)(_.A,{href:t,className:(0,u.A)(T.tag,a?T.tagWithCount:T.tagRegular),children:[s,a&&(0,l.jsx)("span",{children:a})]})}const k={tags:"tags_jXut",tag:"tag_QGVx"};function w(e){let{tags:t}=e;return(0,l.jsxs)(l.Fragment,{children:[(0,l.jsx)("b",{children:(0,l.jsx)(j.A,{id:"theme.tags.tagsListLabel",description:"The label alongside a tag list",children:"Tags:"})}),(0,l.jsx)("ul",{className:(0,u.A)(k.tags,"padding--none","margin-left--sm"),children:t.map((e=>{let{label:t,permalink:s}=e;return(0,l.jsx)("li",{className:k.tag,children:(0,l.jsx)(U,{label:t,permalink:s})},s)}))})]})}const y={lastUpdated:"lastUpdated_vwxv"};function M(e){return(0,l.jsx)("div",{className:(0,u.A)(v.G.docs.docFooterTagsRow,"row margin-bottom--sm"),children:(0,l.jsx)("div",{className:"col",children:(0,l.jsx)(w,{...e})})})}function B(e){let{editUrl:t,lastUpdatedAt:s,lastUpdatedBy:a,formattedLastUpdatedAt:n}=e;return(0,l.jsxs)("div",{className:(0,u.A)(v.G.docs.docFooterEditMetaRow,"row"),children:[(0,l.jsx)("div",{className:"col",children:t&&(0,l.jsx)(C,{editUrl:t})}),(0,l.jsx)("div",{className:(0,u.A)("col",y.lastUpdated),children:(s||a)&&(0,l.jsx)(A,{lastUpdatedAt:s,formattedLastUpdatedAt:n,lastUpdatedBy:a})})]})}function I(){const{metadata:e}=d(),{editUrl:t,lastUpdatedAt:s,formattedLastUpdatedAt:a,lastUpdatedBy:n,tags:i}=e,o=i.length>0,r=!!(t||s||n);return o||r?(0,l.jsxs)("footer",{className:(0,u.A)(v.G.docs.docFooter,"docusaurus-mt-lg"),children:[o&&(0,l.jsx)(M,{tags:i}),r&&(0,l.jsx)(B,{editUrl:t,lastUpdatedAt:s,lastUpdatedBy:n,formattedLastUpdatedAt:a})]}):null}var V=s(1422),E=s(5195);const H={tocCollapsibleButton:"tocCollapsibleButton_TO0P",tocCollapsibleButtonExpanded:"tocCollapsibleButtonExpanded_MG3E"};function G(e){let{collapsed:t,...s}=e;return(0,l.jsx)("button",{type:"button",...s,className:(0,u.A)("clean-btn",H.tocCollapsibleButton,!t&&H.tocCollapsibleButtonExpanded,s.className),children:(0,l.jsx)(j.A,{id:"theme.TOCCollapsible.toggleButtonLabel",description:"The label used by the button on the collapsible TOC component",children:"On this page"})})}const P={tocCollapsible:"tocCollapsible_ETCw",tocCollapsibleContent:"tocCollapsibleContent_vkbj",tocCollapsibleExpanded:"tocCollapsibleExpanded_sAul"};function D(e){let{toc:t,className:s,minHeadingLevel:a,maxHeadingLevel:n}=e;const{collapsed:i,toggleCollapsed:o}=(0,V.u)({initialState:!0});return(0,l.jsxs)("div",{className:(0,u.A)(P.tocCollapsible,!i&&P.tocCollapsibleExpanded,s),children:[(0,l.jsx)(G,{collapsed:i,onClick:o}),(0,l.jsx)(V.N,{lazy:!0,className:P.tocCollapsibleContent,collapsed:i,children:(0,l.jsx)(E.A,{toc:t,minHeadingLevel:a,maxHeadingLevel:n})})]})}const S={tocMobile:"tocMobile_ITEo"};function F(){const{toc:e,frontMatter:t}=d();return(0,l.jsx)(D,{toc:e,minHeadingLevel:t.toc_min_heading_level,maxHeadingLevel:t.toc_max_heading_level,className:(0,u.A)(v.G.docs.docTocMobile,S.tocMobile)})}var R=s(7763);function O(){const{toc:e,frontMatter:t}=d();return(0,l.jsx)(R.A,{toc:e,minHeadingLevel:t.toc_min_heading_level,maxHeadingLevel:t.toc_max_heading_level,className:v.G.docs.docTocDesktop})}var z=s(1107),W=s(2480);function q(e){let{children:t}=e;const s=function(){const{metadata:e,frontMatter:t,contentTitle:s}=d();return t.hide_title||void 0!==s?null:e.title}();return(0,l.jsxs)("div",{className:(0,u.A)(v.G.docs.docMarkdown,"markdown"),children:[s&&(0,l.jsx)("header",{children:(0,l.jsx)(z.A,{as:"h1",children:s})}),(0,l.jsx)(W.A,{children:t})]})}var Z=s(1243),Q=s(996);const X={docItemContainer:"docItemContainer_Djhp",docItemCol:"docItemCol_VOVn"};function Y(e){let{children:t}=e;const s=function(){const{frontMatter:e,toc:t}=d(),s=(0,m.l)(),a=e.hide_table_of_contents,n=!a&&t.length>0;return{hidden:a,mobile:n?(0,l.jsx)(F,{}):void 0,desktop:!n||"desktop"!==s&&"ssr"!==s?void 0:(0,l.jsx)(O,{})}}(),{metadata:{unlisted:a}}=d();return(0,l.jsxs)("div",{className:"row",children:[(0,l.jsxs)("div",{className:(0,u.A)("col",!s.hidden&&X.docItemCol),children:[a&&(0,l.jsx)(Q.A,{}),(0,l.jsx)(p.A,{}),(0,l.jsxs)("div",{className:X.docItemContainer,children:[(0,l.jsxs)("article",{children:[(0,l.jsx)(Z.A,{}),(0,l.jsx)(x.A,{}),s.mobile,(0,l.jsx)(q,{children:t}),(0,l.jsx)(I,{})]}),(0,l.jsx)(b,{})]})]}),s.desktop&&(0,l.jsx)("div",{className:"col col--3",children:s.desktop})]})}function $(e){const t=`docs-doc-id-${e.content.metadata.id}`,s=e.content;return(0,l.jsx)(r,{content:e.content,children:(0,l.jsxs)(n.e3,{className:t,children:[(0,l.jsx)(c,{}),(0,l.jsx)(Y,{children:(0,l.jsx)(s,{})})]})})}},6929:(e,t,s)=>{s.d(t,{A:()=>r});s(6540);var a=s(1312),n=s(8215),i=s(8774),l=s(4848);function o(e){const{permalink:t,title:s,subLabel:a,isNext:o}=e;return(0,l.jsxs)(i.A,{className:(0,n.A)("pagination-nav__link",o?"pagination-nav__link--next":"pagination-nav__link--prev"),to:t,children:[a&&(0,l.jsx)("div",{className:"pagination-nav__sublabel",children:a}),(0,l.jsx)("div",{className:"pagination-nav__label",children:s})]})}function r(e){const{previous:t,next:s}=e;return(0,l.jsxs)("nav",{className:"pagination-nav docusaurus-mt-lg","aria-label":(0,a.T)({id:"theme.docs.paginator.navAriaLabel",message:"Docs pages",description:"The ARIA label for the docs pagination"}),children:[t&&(0,l.jsx)(o,{...t,subLabel:(0,l.jsx)(a.A,{id:"theme.docs.paginator.previous",description:"The label used to navigate to the previous doc",children:"Previous"})}),s&&(0,l.jsx)(o,{...s,subLabel:(0,l.jsx)(a.A,{id:"theme.docs.paginator.next",description:"The label used to navigate to the next doc",children:"Next"}),isNext:!0})]})}},4267:(e,t,s)=>{s.d(t,{A:()=>r});s(6540);var a=s(8215),n=s(1312),i=s(7559),l=s(2252),o=s(4848);function r(e){let{className:t}=e;const s=(0,l.r)();return s.badge?(0,o.jsx)("span",{className:(0,a.A)(t,i.G.docs.docVersionBadge,"badge badge--secondary"),children:(0,o.jsx)(n.A,{id:"theme.docs.versionBadge.label",values:{versionLabel:s.label},children:"Version: {versionLabel}"})}):null}},1878:(e,t,s)=>{s.d(t,{A:()=>x});s(6540);var a=s(8215),n=s(4586),i=s(8774),l=s(1312),o=s(4070),r=s(7559),d=s(5597),c=s(2252),u=s(4848);const m={unreleased:function(e){let{siteTitle:t,versionMetadata:s}=e;return(0,u.jsx)(l.A,{id:"theme.docs.versions.unreleasedVersionLabel",description:"The label used to tell the user that he's browsing an unreleased doc version",values:{siteTitle:t,versionLabel:(0,u.jsx)("b",{children:s.label})},children:"This is unreleased documentation for {siteTitle} {versionLabel} version."})},unmaintained:function(e){let{siteTitle:t,versionMetadata:s}=e;return(0,u.jsx)(l.A,{id:"theme.docs.versions.unmaintainedVersionLabel",description:"The label used to tell the user that he's browsing an unmaintained doc version",values:{siteTitle:t,versionLabel:(0,u.jsx)("b",{children:s.label})},children:"This is documentation for {siteTitle} {versionLabel}, which is no longer actively maintained."})}};function h(e){const t=m[e.versionMetadata.banner];return(0,u.jsx)(t,{...e})}function b(e){let{versionLabel:t,to:s,onClick:a}=e;return(0,u.jsx)(l.A,{id:"theme.docs.versions.latestVersionSuggestionLabel",description:"The label used to tell the user to check the latest version",values:{versionLabel:t,latestVersionLink:(0,u.jsx)("b",{children:(0,u.jsx)(i.A,{to:s,onClick:a,children:(0,u.jsx)(l.A,{id:"theme.docs.versions.latestVersionLinkLabel",description:"The label used for the latest version suggestion link label",children:"latest version"})})})},children:"For up-to-date documentation, see the {latestVersionLink} ({versionLabel})."})}function p(e){let{className:t,versionMetadata:s}=e;const{siteConfig:{title:i}}=(0,n.A)(),{pluginId:l}=(0,o.vT)({failfast:!0}),{savePreferredVersionName:c}=(0,d.g1)(l),{latestDocSuggestion:m,latestVersionSuggestion:p}=(0,o.HW)(l),x=m??(v=p).docs.find((e=>e.id===v.mainDocId));var v;return(0,u.jsxs)("div",{className:(0,a.A)(t,r.G.docs.docVersionBanner,"alert alert--warning margin-bottom--md"),role:"alert",children:[(0,u.jsx)("div",{children:(0,u.jsx)(h,{siteTitle:i,versionMetadata:s})}),(0,u.jsx)("div",{className:"margin-top--md",children:(0,u.jsx)(b,{versionLabel:p.label,to:x.path,onClick:()=>c(p.name)})})]})}function x(e){let{className:t}=e;const s=(0,c.r)();return s.banner?(0,u.jsx)(p,{className:t,versionMetadata:s}):null}}}]);