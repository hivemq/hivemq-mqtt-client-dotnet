"use strict";(self.webpackChunkdocumentation=self.webpackChunkdocumentation||[]).push([[369],{8080:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>r,contentTitle:()=>c,default:()=>d,frontMatter:()=>i,metadata:()=>a,toc:()=>u});var s=n(5893),o=n(1151);const i={},c="Subscribe & Publish",a={id:"how-to/sub-and-pub",title:"Subscribe & Publish",description:"Once you set a message handler in OnMessageReceived, you can call SubscribeAsync to subscribe to one or more topics.",source:"@site/docs/how-to/sub-and-pub.md",sourceDirName:"how-to",slug:"/how-to/sub-and-pub",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/sub-and-pub",draft:!1,unlisted:!1,editUrl:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Documentation/docs/how-to/sub-and-pub.md",tags:[],version:"current",frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"How to set a Last Will & Testament",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/set-lwt"},next:{title:"Subscribe to Multiple Topics At Once With Varying QoS Levels",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/subscribe-multi"}},r={},u=[];function l(e){const t={code:"code",h1:"h1",p:"p",pre:"pre",...(0,o.a)(),...e.components};return(0,s.jsxs)(s.Fragment,{children:[(0,s.jsx)(t.h1,{id:"subscribe--publish",children:"Subscribe & Publish"}),"\n",(0,s.jsxs)(t.p,{children:["Once you set a message handler in ",(0,s.jsx)(t.code,{children:"OnMessageReceived"}),", you can call ",(0,s.jsx)(t.code,{children:"SubscribeAsync"})," to subscribe to one or more topics."]}),"\n",(0,s.jsxs)(t.p,{children:["Note that you should always set the handler before subscribing when possible.  This avoids the case of lost messages as the broker can send messages immediately after ",(0,s.jsx)(t.code,{children:"SubscribeAsync"}),"."]}),"\n",(0,s.jsx)(t.pre,{children:(0,s.jsx)(t.code,{className:"language-csharp",children:'using HiveMQtt.Client;\n\n// Connect\nvar client = new HiveMQClient();\nvar connectResult = await client.ConnectAsync().ConfigureAwait(false);\n\n// Message Handler\nclient.OnMessageReceived += (sender, args) =>\n{\n    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString)\n};\n\n// Subscribe\nawait client.SubscribeAsync("instrument/x9284/boston").ConfigureAwait(false);\n\nawait client.PublishAsync(\n                "core/dynamic_graph/entity/227489", // Topic to publish to\n                "{\'2023\': \'\ud83d\udc4d\'}"                    // Message to publish\n                ).ConfigureAwait(false);\n'})})]})}function d(e={}){const{wrapper:t}={...(0,o.a)(),...e.components};return t?(0,s.jsx)(t,{...e,children:(0,s.jsx)(l,{...e})}):l(e)}},1151:(e,t,n)=>{n.d(t,{Z:()=>a,a:()=>c});var s=n(7294);const o={},i=s.createContext(o);function c(e){const t=s.useContext(i);return s.useMemo((function(){return"function"==typeof e?e(t):{...t,...e}}),[t,e])}function a(e){let t;return t=e.disableParentContext?"function"==typeof e.components?e.components(o):e.components||o:c(e.components),s.createElement(i.Provider,{value:t},e.children)}}}]);