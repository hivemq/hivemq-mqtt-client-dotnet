"use strict";(self.webpackChunkdocumentation=self.webpackChunkdocumentation||[]).push([[386],{439:(t,e,n)=>{n.r(e),n.d(e,{assets:()=>a,contentTitle:()=>s,default:()=>d,frontMatter:()=>c,metadata:()=>r,toc:()=>l});var o=n(5893),i=n(1151);const c={},s="Securely Connect to a Broker with Basic Authentication Credentials",r={id:"how-to/connect-with-auth",title:"Securely Connect to a Broker with Basic Authentication Credentials",description:"To securely connect to an MQTT Broker with basic authentication credentials, use the UserName and Password fields in HiveMQClientOptions:",source:"@site/docs/how-to/connect-with-auth.md",sourceDirName:"how-to",slug:"/how-to/connect-with-auth",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/connect-with-auth",draft:!1,unlisted:!1,editUrl:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Documentation/docs/how-to/connect-with-auth.md",tags:[],version:"current",frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"Configure HiveMQtt Logging",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/configure-logging"},next:{title:"How to set a Last Will & Testament",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/set-lwt"}},a={},l=[];function u(t){const e={code:"code",h1:"h1",p:"p",pre:"pre",...(0,i.a)(),...t.components};return(0,o.jsxs)(o.Fragment,{children:[(0,o.jsx)(e.h1,{id:"securely-connect-to-a-broker-with-basic-authentication-credentials",children:"Securely Connect to a Broker with Basic Authentication Credentials"}),"\n",(0,o.jsxs)(e.p,{children:["To securely connect to an MQTT Broker with basic authentication credentials, use the ",(0,o.jsx)(e.code,{children:"UserName"})," and ",(0,o.jsx)(e.code,{children:"Password"})," fields in ",(0,o.jsx)(e.code,{children:"HiveMQClientOptions"}),":"]}),"\n",(0,o.jsx)(e.pre,{children:(0,o.jsx)(e.code,{className:"language-csharp",children:'var options = new HiveMQClientOptions()\n{\n    Host = "b8293h09193b.s1.eu.hivemq.cloud",\n    Port = 8883,\n    UseTLS = true,\n    UserName = "my-username",\n    Password = "my-password",\n};\n\nvar client = new HiveMQClient(options);\nvar connectResult = await client.ConnectAsync().ConfigureAwait(false);\n'})})]})}function d(t={}){const{wrapper:e}={...(0,i.a)(),...t.components};return e?(0,o.jsx)(e,{...t,children:(0,o.jsx)(u,{...t})}):u(t)}},1151:(t,e,n)=>{n.d(e,{Z:()=>r,a:()=>s});var o=n(7294);const i={},c=o.createContext(i);function s(t){const e=o.useContext(c);return o.useMemo((function(){return"function"==typeof t?t(e):{...e,...t}}),[e,t])}function r(t){let e;return e=t.disableParentContext?"function"==typeof t.components?t.components(i):t.components||i:s(t.components),o.createElement(c.Provider,{value:e},t.children)}}}]);