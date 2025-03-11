"use strict";(self.webpackChunkdocumentation=self.webpackChunkdocumentation||[]).push([[1736],{7219:(e,n,s)=>{s.r(n),s.d(n,{assets:()=>l,contentTitle:()=>c,default:()=>p,frontMatter:()=>o,metadata:()=>r,toc:()=>d});var t=s(4848),i=s(8453);const o={},c="ConnectOptions",r={id:"reference/connect_options",title:"ConnectOptions",description:"The ConnectOptions class provides options for a connect call made with ConnectAsync. These options can override settings that were originally set in HiveMQClientOptions.",source:"@site/docs/reference/connect_options.md",sourceDirName:"reference",slug:"/reference/connect_options",permalink:"/hivemq-mqtt-client-dotnet/docs/reference/connect_options",draft:!1,unlisted:!1,editUrl:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Documentation/docs/reference/connect_options.md",tags:[],version:"current",frontMatter:{},sidebar:"docsSidebar",previous:{title:"HiveMQClientOptionsBuilder",permalink:"/hivemq-mqtt-client-dotnet/docs/reference/client_options_builder"},next:{title:"ConnectOptionsBuilder",permalink:"/hivemq-mqtt-client-dotnet/docs/reference/connect_options_builder"}},l={},d=[{value:"Constructors",id:"constructors",level:2},{value:"Properties",id:"properties",level:2},{value:"Examples",id:"examples",level:2},{value:"See Also",id:"see-also",level:2}];function a(e){const n={a:"a",code:"code",h1:"h1",h2:"h2",li:"li",p:"p",pre:"pre",ul:"ul",...(0,i.R)(),...e.components};return(0,t.jsxs)(t.Fragment,{children:[(0,t.jsx)(n.h1,{id:"connectoptions",children:"ConnectOptions"}),"\n",(0,t.jsxs)(n.p,{children:["The ",(0,t.jsx)(n.code,{children:"ConnectOptions"})," class provides options for a connect call made with ",(0,t.jsx)(n.code,{children:"ConnectAsync"}),". These options can override settings that were originally set in ",(0,t.jsx)(n.code,{children:"HiveMQClientOptions"}),"."]}),"\n",(0,t.jsx)(n.h2,{id:"constructors",children:"Constructors"}),"\n",(0,t.jsxs)(n.ul,{children:["\n",(0,t.jsxs)(n.li,{children:[(0,t.jsx)(n.code,{children:"ConnectOptions()"}),": Initializes a new instance of the ",(0,t.jsx)(n.code,{children:"ConnectOptions"})," class with defaults."]}),"\n"]}),"\n",(0,t.jsx)(n.h2,{id:"properties",children:"Properties"}),"\n",(0,t.jsxs)(n.ul,{children:["\n",(0,t.jsxs)(n.li,{children:["\n",(0,t.jsxs)(n.p,{children:[(0,t.jsx)(n.code,{children:"SessionExpiryInterval"}),": Gets or sets the session expiry interval in seconds. This overrides any value set in HiveMQClientOptions."]}),"\n",(0,t.jsxs)(n.ul,{children:["\n",(0,t.jsxs)(n.li,{children:["Example: ",(0,t.jsx)(n.code,{children:"SessionExpiryInterval = 3600"})," sets the session to expire in 1 hour."]}),"\n"]}),"\n"]}),"\n",(0,t.jsxs)(n.li,{children:["\n",(0,t.jsxs)(n.p,{children:[(0,t.jsx)(n.code,{children:"KeepAlive"}),": Gets or sets the keep alive period in seconds. This overrides any value set in HiveMQClientOptions."]}),"\n",(0,t.jsxs)(n.ul,{children:["\n",(0,t.jsxs)(n.li,{children:["Example: ",(0,t.jsx)(n.code,{children:"KeepAlive = 60"})," sets the keep alive to 60 seconds."]}),"\n"]}),"\n"]}),"\n",(0,t.jsxs)(n.li,{children:["\n",(0,t.jsxs)(n.p,{children:[(0,t.jsx)(n.code,{children:"CleanStart"}),": Gets or sets whether to use a clean start. This overrides any value set in HiveMQClientOptions."]}),"\n",(0,t.jsxs)(n.ul,{children:["\n",(0,t.jsxs)(n.li,{children:["Example: ",(0,t.jsx)(n.code,{children:"CleanStart = true"})," starts a new session, discarding any existing session."]}),"\n"]}),"\n"]}),"\n"]}),"\n",(0,t.jsx)(n.h2,{id:"examples",children:"Examples"}),"\n",(0,t.jsx)(n.pre,{children:(0,t.jsx)(n.code,{className:"language-csharp",children:"ConnectOptions connectOptions = new ConnectOptions();\nconnectOptions.SessionExpiryInterval = 3600;  // 1 hour session expiry\nconnectOptions.KeepAlive = 60;                // 60 second keep alive\nconnectOptions.CleanStart = true;             // Start with a clean session\n\nawait client.ConnectAsync(connectOptions);\n"})}),"\n",(0,t.jsx)(n.h2,{id:"see-also",children:"See Also"}),"\n",(0,t.jsxs)(n.ul,{children:["\n",(0,t.jsx)(n.li,{children:(0,t.jsx)(n.a,{href:"/docs/reference/client_options",children:"HiveMQClientOptions Reference"})}),"\n",(0,t.jsx)(n.li,{children:(0,t.jsx)(n.a,{href:"/docs/connecting",children:"Connecting to an MQTT Broker"})}),"\n",(0,t.jsx)(n.li,{children:(0,t.jsx)(n.a,{href:"/docs/how-to/session-handling",children:"Session Handling"})}),"\n"]})]})}function p(e={}){const{wrapper:n}={...(0,i.R)(),...e.components};return n?(0,t.jsx)(n,{...e,children:(0,t.jsx)(a,{...e})}):a(e)}},8453:(e,n,s)=>{s.d(n,{R:()=>c,x:()=>r});var t=s(6540);const i={},o=t.createContext(i);function c(e){const n=t.useContext(o);return t.useMemo((function(){return"function"==typeof e?e(n):{...n,...e}}),[n,e])}function r(e){let n;return n=e.disableParentContext?"function"==typeof e.components?e.components(i):e.components||i:c(e.components),t.createElement(o.Provider,{value:n},e.children)}}}]);