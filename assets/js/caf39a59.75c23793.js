"use strict";(self.webpackChunkdocumentation=self.webpackChunkdocumentation||[]).push([[982],{7037:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>d,contentTitle:()=>l,default:()=>x,frontMatter:()=>r,metadata:()=>h,toc:()=>c});var s=n(5893),i=n(1151);const r={sidebar_position:90},l="Benchmarks",h={id:"benchmarks",title:"Benchmarks",description:"The benchmarks provided in the HiveMQtt GitHub repository are built using BenchmarkDotNet, a .NET library for benchmarking. These benchmarks are designed to measure the performance of various messaging operations against any MQTT broker.",source:"@site/docs/benchmarks.md",sourceDirName:".",slug:"/benchmarks",permalink:"/hivemq-mqtt-client-dotnet/docs/benchmarks",draft:!1,unlisted:!1,editUrl:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Documentation/docs/benchmarks.md",tags:[],version:"current",sidebarPosition:90,frontMatter:{sidebar_position:90},sidebar:"docsSidebar",previous:{title:"DisconnectOptions",permalink:"/hivemq-mqtt-client-dotnet/docs/reference/disconnect_options"},next:{title:"Getting Help",permalink:"/hivemq-mqtt-client-dotnet/docs/help"}},d={},c=[{value:"Running Benchmarks",id:"running-benchmarks",level:2},{value:"Results",id:"results",level:2},{value:"Legend",id:"legend",level:2},{value:"Mar 22, 2024",id:"mar-22-2024",level:2},{value:"Mar 21, 2024",id:"mar-21-2024",level:2},{value:"Previous Performance",id:"previous-performance",level:3},{value:"First Pass Refactor Performance",id:"first-pass-refactor-performance",level:3},{value:"Final Refactor Performance Results (for now \ud83d\udc7b)",id:"final-refactor-performance-results-for-now-",level:3}];function a(e){const t={a:"a",code:"code",h1:"h1",h2:"h2",h3:"h3",p:"p",pre:"pre",strong:"strong",table:"table",tbody:"tbody",td:"td",th:"th",thead:"thead",tr:"tr",...(0,i.a)(),...e.components};return(0,s.jsxs)(s.Fragment,{children:[(0,s.jsx)(t.h1,{id:"benchmarks",children:"Benchmarks"}),"\n",(0,s.jsx)(t.p,{children:"The benchmarks provided in the HiveMQtt GitHub repository are built using BenchmarkDotNet, a .NET library for benchmarking. These benchmarks are designed to measure the performance of various messaging operations against any MQTT broker."}),"\n",(0,s.jsx)(t.h2,{id:"running-benchmarks",children:"Running Benchmarks"}),"\n",(0,s.jsx)(t.p,{children:"To run the benchmarks, execute the following commands:"}),"\n",(0,s.jsx)(t.pre,{children:(0,s.jsx)(t.code,{className:"language-bash",children:"cd Benchmarks/ClientBenchmarkApp\ndotnet run ClientBenchmarkApp.csproj -c Release\n"})}),"\n",(0,s.jsx)(t.h2,{id:"results",children:"Results"}),"\n",(0,s.jsx)(t.p,{children:"The benchmarks provide insights into the performance of different messaging methods under various scenarios. Below are the results obtained from running the benchmarks on a local MBP (MacBook Pro) against a HiveMQ v4 broker running in a Docker container over localhost."}),"\n",(0,s.jsx)(t.h2,{id:"legend",children:"Legend"}),"\n",(0,s.jsx)(t.pre,{children:(0,s.jsx)(t.code,{children:"  Mean   : Arithmetic mean of all measurements\n  Error  : Half of 99.9% confidence interval\n  StdDev : Standard deviation of all measurements\n  Median : Value separating the higher half of all measurements (50th percentile)\n  1 us   : 1 Microsecond (0.000001 sec)\n  1 ms   : 1,000 Microseconds\n"})}),"\n",(0,s.jsx)(t.h2,{id:"mar-22-2024",children:"Mar 22, 2024"}),"\n",(0,s.jsxs)(t.p,{children:[(0,s.jsx)(t.strong,{children:"Publishing a QoS 2 message"})," with the full round-trip of confirmation packets ",(0,s.jsx)(t.strong,{children:"is now timed at ~1.2ms"}),"."]}),"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n",(0,s.jsxs)(t.table,{children:[(0,s.jsx)(t.thead,{children:(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.th,{children:"Method"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Mean"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Error"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"StdDev"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Median"})]})}),(0,s.jsxs)(t.tbody,{children:[(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 0 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"57.27 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"158.55 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"467.50 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"9.084 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 1 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"2,291.28 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"903.01 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"2,662.56 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,357.063 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 2 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"2,058.05 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,048.91 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"3,092.73 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,292.396 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish 100 256b length payload QoS 0 messages'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"138.29 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"183.38 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"540.69 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"79.604 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish 100 256b length payload QoS 1 messages'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"45,813.98 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"4,838.62 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"14,266.78 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"42,482.520 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish 100 256b length payload QoS 2 messages'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"88,589.38 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"3,877.02 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"11,431.48 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"85,640.167 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish 100 256k length payload QoS 0 messages'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"124.92 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"173.22 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"510.74 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"69.709 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish 100 256k length payload QoS 1 messages'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"270,043.05 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"8,850.72 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"26,096.56 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"266,506.583 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish 100 256k length payload QoS 2 messages'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"300,923.38 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"5,704.22 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"16,819.03 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"296,254.688 us"})]})]})]}),"\n",(0,s.jsxs)(t.p,{children:["See also: ",(0,s.jsx)(t.a,{href:"https://www.hivemq.com/blog/mqtt-essentials-part-6-mqtt-quality-of-service-levels/",children:"What is MQTT Quality of Service (QoS) 0,1, & 2? \u2013 MQTT Essentials: Part 6"}),"."]}),"\n",(0,s.jsx)(t.h2,{id:"mar-21-2024",children:"Mar 21, 2024"}),"\n",(0,s.jsxs)(t.p,{children:["With release ",(0,s.jsx)(t.a,{href:"https://github.com/hivemq/hivemq-mqtt-client-dotnet/releases/tag/v0.11.0",children:"v0.11.0"})," there was a big performance improvement.  All messaging performance was improved but particularly publishing a QoS level 2 message went from ~206ms down to ~1.6ms."]}),"\n",(0,s.jsx)(t.h3,{id:"previous-performance",children:"Previous Performance"}),"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n",(0,s.jsxs)(t.table,{children:[(0,s.jsx)(t.thead,{children:(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.th,{children:"Method"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Mean"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Error"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"StdDev"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Median"})]})}),(0,s.jsxs)(t.tbody,{children:[(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 0 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"390.8 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,842.5 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,218.7 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"5.646 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 1 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"103,722.8 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"4,330.0 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"2,864.1 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"103,536.375 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 2 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"202,367.9 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"26,562.9 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"17,569.7 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"206,959.834 us"})]})]})]}),"\n",(0,s.jsx)(t.h3,{id:"first-pass-refactor-performance",children:"First Pass Refactor Performance"}),"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n",(0,s.jsxs)(t.table,{children:[(0,s.jsx)(t.thead,{children:(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.th,{children:"Method"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Mean"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Error"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"StdDev"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Median"})]})}),(0,s.jsxs)(t.tbody,{children:[(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 0 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"401.9 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,876.3 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,241.0 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"9.250 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 1 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"2,140.0 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"3,568.2 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"2,360.1 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,324.251 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 2 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"4,217.2 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"5,803.7 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"3,838.8 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"2,569.166 us"})]})]})]}),"\n",(0,s.jsx)(t.h3,{id:"final-refactor-performance-results-for-now-",children:"Final Refactor Performance Results (for now \ud83d\udc7b)"}),"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n",(0,s.jsxs)(t.table,{children:[(0,s.jsx)(t.thead,{children:(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.th,{children:"Method"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Mean"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Error"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"StdDev"}),(0,s.jsx)(t.th,{style:{textAlign:"right"},children:"Median"})]})}),(0,s.jsxs)(t.tbody,{children:[(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 0 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"47.11 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"139.47 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"411.23 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"4.875 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 1 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,210.71 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"508.64 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,499.75 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"790.645 us"})]}),(0,s.jsxs)(t.tr,{children:[(0,s.jsx)(t.td,{children:"'Publish a QoS 2 message'"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"2,080.46 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"591.38 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,743.71 us"}),(0,s.jsx)(t.td,{style:{textAlign:"right"},children:"1,653.083 us"})]})]})]})]})}function x(e={}){const{wrapper:t}={...(0,i.a)(),...e.components};return t?(0,s.jsx)(t,{...e,children:(0,s.jsx)(a,{...e})}):a(e)}},1151:(e,t,n)=>{n.d(t,{Z:()=>h,a:()=>l});var s=n(7294);const i={},r=s.createContext(i);function l(e){const t=s.useContext(r);return s.useMemo((function(){return"function"==typeof e?e(t):{...t,...e}}),[t,e])}function h(e){let t;return t=e.disableParentContext?"function"==typeof e.components?e.components(i):e.components||i:l(e.components),s.createElement(r.Provider,{value:t},e.children)}}}]);