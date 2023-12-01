"use strict";(self.webpackChunkdocumentation=self.webpackChunkdocumentation||[]).push([[775],{7880:(t,e,i)=>{i.r(e),i.d(e,{assets:()=>l,contentTitle:()=>c,default:()=>d,frontMatter:()=>o,metadata:()=>r,toc:()=>u});var n=i(5893),s=i(1151);const o={},c="Subscribe to Multiple Topics At Once With Varying QoS Levels",r={id:"how-to/subscribe-multi",title:"Subscribe to Multiple Topics At Once With Varying QoS Levels",description:"* result.Subscriptions contains the list of subscriptions made with this call",source:"@site/docs/how-to/subscribe-multi.md",sourceDirName:"how-to",slug:"/how-to/subscribe-multi",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/subscribe-multi",draft:!1,unlisted:!1,editUrl:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Documentation/docs/how-to/subscribe-multi.md",tags:[],version:"current",frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"Subscribe & Publish",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/sub-and-pub"},next:{title:"Wait on an Event",permalink:"/hivemq-mqtt-client-dotnet/docs/how-to/wait-on-event"}},l={},u=[];function a(t){const e={code:"code",h1:"h1",li:"li",pre:"pre",ul:"ul",...(0,s.a)(),...t.components};return(0,n.jsxs)(n.Fragment,{children:[(0,n.jsx)(e.h1,{id:"subscribe-to-multiple-topics-at-once-with-varying-qos-levels",children:"Subscribe to Multiple Topics At Once With Varying QoS Levels"}),"\n",(0,n.jsx)(e.pre,{children:(0,n.jsx)(e.code,{className:"language-csharp",children:'using HiveMQtt.Client.Options;\nusing HiveMQtt.Client.Results;\n\nvar options = new SubscribeOptions();\noptions.TopicFilters.Add(new TopicFilter { Topic = "foo/boston", QoS = QualityOfService.AtLeastOnceDelivery });\noptions.TopicFilters.Add(new TopicFilter { Topic = "bar/landshut", QoS = QualityOfService.AtMostOnceDelivery });\n\nvar result = await client.SubscribeAsync(options);\n'})}),"\n",(0,n.jsxs)(e.ul,{children:["\n",(0,n.jsxs)(e.li,{children:[(0,n.jsx)(e.code,{children:"result.Subscriptions"})," contains the list of subscriptions made with this call"]}),"\n",(0,n.jsxs)(e.li,{children:[(0,n.jsx)(e.code,{children:"client.Subscriptions"})," is updated with complete list of subscriptions made up to this point"]}),"\n",(0,n.jsxs)(e.li,{children:["each ",(0,n.jsx)(e.code,{children:"Subscription"})," object has a resulting ",(0,n.jsx)(e.code,{children:"ReasonCode"})," that represents the Subscribe result in ",(0,n.jsx)(e.code,{children:"result.Subscriptions[0].ReasonCode"})]}),"\n"]})]})}function d(t={}){const{wrapper:e}={...(0,s.a)(),...t.components};return e?(0,n.jsx)(e,{...t,children:(0,n.jsx)(a,{...t})}):a(t)}},1151:(t,e,i)=>{i.d(e,{Z:()=>r,a:()=>c});var n=i(7294);const s={},o=n.createContext(s);function c(t){const e=n.useContext(o);return n.useMemo((function(){return"function"==typeof t?t(e):{...e,...t}}),[e,t])}function r(t){let e;return e=t.disableParentContext?"function"==typeof t.components?t.components(s):t.components||s:c(t.components),n.createElement(o.Provider,{value:e},t.children)}}}]);