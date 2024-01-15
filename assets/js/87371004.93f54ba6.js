"use strict";(self.webpackChunkdocumentation=self.webpackChunkdocumentation||[]).push([[386],{439:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>a,contentTitle:()=>s,default:()=>d,frontMatter:()=>c,metadata:()=>r,toc:()=>h});var i=n(5893),o=n(1151);const c={},s="Securely Connect to a Broker with Basic Authentication Credentials",r={id:"how-to/connect-with-auth",title:"Securely Connect to a Broker with Basic Authentication Credentials",description:"To securely connect to an MQTT Broker with basic authentication credentials, use the UserName and Password fields in HiveMQClientOptions:",source:"@site/docs/how-to/connect-with-auth.md",sourceDirName:"how-to",slug:"/how-to/connect-with-auth",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/connect-with-auth",draft:!1,unlisted:!1,editUrl:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Documentation/docs/how-to/connect-with-auth.md",tags:[],version:"current",frontMatter:{},sidebar:"docsSidebar",previous:{title:"Configure Logging",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/configure-logging"},next:{title:"How to Debug",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/debug"}},a={},h=[{value:"See Also",id:"see-also",level:2}];function l(e){const t={a:"a",code:"code",h1:"h1",h2:"h2",li:"li",p:"p",pre:"pre",ul:"ul",...(0,o.a)(),...e.components};return(0,i.jsxs)(i.Fragment,{children:[(0,i.jsx)(t.h1,{id:"securely-connect-to-a-broker-with-basic-authentication-credentials",children:"Securely Connect to a Broker with Basic Authentication Credentials"}),"\n",(0,i.jsxs)(t.p,{children:["To securely connect to an MQTT Broker with basic authentication credentials, use the ",(0,i.jsx)(t.code,{children:"UserName"})," and ",(0,i.jsx)(t.code,{children:"Password"})," fields in ",(0,i.jsx)(t.code,{children:"HiveMQClientOptions"}),":"]}),"\n",(0,i.jsx)(t.pre,{children:(0,i.jsx)(t.code,{className:"language-csharp",children:'var options = new HiveMQClientOptionsBuilder()\n    .WithBroker("b273h09193b.s1.eu.hivemq.cloud")\n    .WithPort(8883)\n    .WithUseTls(true)\n    .WithUserName("my-username")\n    .WithPassword("my-password")\n    .Build();\n\nvar client = new HiveMQClient(options);\nvar connectResult = await client.ConnectAsync().ConfigureAwait(false);\n'})}),"\n",(0,i.jsx)(t.h2,{id:"see-also",children:"See Also"}),"\n",(0,i.jsxs)(t.ul,{children:["\n",(0,i.jsx)(t.li,{children:(0,i.jsx)(t.a,{href:"https://www.hivemq.com/blog/mqtt-security-fundamentals-authentication-username-password/",children:"Authentication with Username and Password - MQTT Security Fundamentals"})}),"\n",(0,i.jsx)(t.li,{children:(0,i.jsx)(t.a,{href:"https://www.hivemq.com/blog/mqtt-security-fundamentals-advanced-authentication-mechanisms/",children:"Advanced Authentication Mechanisms - MQTT Security Fundamentals"})}),"\n",(0,i.jsx)(t.li,{children:(0,i.jsx)(t.a,{href:"https://docs.hivemq.com/hivemq-cloud/authn-authz.html",children:"HiveMQ Cloud / Authentication and Authorization"})}),"\n",(0,i.jsx)(t.li,{children:(0,i.jsx)(t.a,{href:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/HiveMQClientOptionsBuilder.cs",children:"HiveMQClientOptionsBuilder.cs"})}),"\n",(0,i.jsx)(t.li,{children:(0,i.jsx)(t.a,{href:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/blob/main/Source/HiveMQtt/Client/Options/HiveMQClientOptions.cs",children:"HiveMQClientOptions.cs"})}),"\n"]})]})}function d(e={}){const{wrapper:t}={...(0,o.a)(),...e.components};return t?(0,i.jsx)(t,{...e,children:(0,i.jsx)(l,{...e})}):l(e)}},1151:(e,t,n)=>{n.d(t,{Z:()=>r,a:()=>s});var i=n(7294);const o={},c=i.createContext(o);function s(e){const t=i.useContext(c);return i.useMemo((function(){return"function"==typeof e?e(t):{...t,...e}}),[t,e])}function r(e){let t;return t=e.disableParentContext?"function"==typeof e.components?e.components(o):e.components||o:s(e.components),i.createElement(c.Provider,{value:t},e.children)}}}]);