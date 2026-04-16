import Heading from '@theme/Heading';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import CodeBlock from '@theme/CodeBlock';
import styles from './index.module.css';

const installCode = `dotnet add package HiveMQtt`;

const quickStartCode = `using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

// Connect to HiveMQ Cloud (or any MQTT broker)
var options = new HiveMQClientOptionsBuilder()
    .WithBroker("your-cluster.hivemq.cloud")
    .WithPort(8883)
    .WithUseTls(true)
    .WithUserName("your-username")
    .WithPassword("your-password")
    .Build();

var client = new HiveMQClient(options);

// Handle incoming messages
client.OnMessageReceived += (sender, args) => {
    Console.WriteLine($"Received: {args.PublishMessage.PayloadAsString}");
};

await client.ConnectAsync();
await client.SubscribeAsync("sensors/temperature");
await client.PublishAsync("sensors/temperature", "23.5°C");`;

const FeatureList = [
  {
    icon: '🚀',
    title: 'MQTT 5.0 Ready',
    description: 'Fully compliant with the latest MQTT 5.0 specification. Supports all QoS levels, retained messages, and session management.',
  },
  {
    icon: '⚡',
    title: 'High Performance',
    description: 'Asynchronous design optimized for high-throughput and low-latency. Benchmarked for production IoT workloads.',
  },
  {
    icon: '🔒',
    title: 'Enterprise Security',
    description: 'TLS/SSL, X.509 certificates, SecureString password storage, and secure WebSocket (wss://) support.',
  },
  {
    icon: '🌐',
    title: 'Flexible Transport',
    description: 'TCP and WebSocket connections (ws:// and wss://). Works across firewalls and HTTP-only environments.',
  },
  {
    icon: '🎯',
    title: 'Developer Friendly',
    description: 'Intuitive API with smart defaults, builder patterns, and extensive event system for fine-grained control.',
  },
  {
    icon: '📦',
    title: 'Multi-Platform',
    description: 'Supports .NET 6, 7, 8, 9, and 10. Works on Windows, Linux, macOS, and containerized environments.',
  },
];

const UseCaseList = [
  {
    icon: '🏭',
    title: 'Industrial IoT',
    description: 'Connect factory equipment, sensors, and PLCs to your enterprise systems.',
  },
  {
    icon: '🏠',
    title: 'Smart Buildings',
    description: 'Build automation systems for HVAC, lighting, and energy management.',
  },
  {
    icon: '🚗',
    title: 'Connected Vehicles',
    description: 'Real-time telemetry and fleet management for automotive applications.',
  },
  {
    icon: '⚕️',
    title: 'Healthcare',
    description: 'Medical device connectivity and patient monitoring systems.',
  },
];

const ClientOptionsList = [
  {
    icon: '1',
    title: 'HiveMQtt MQTT Client',
    audience: 'Best for most applications',
    description:
      'Use the full-featured MQTT 5 client with subscription tracking, convenient callbacks, and production-ready defaults.',
    href: '/docs/hivemqtt/intro',
    cta: 'Explore HiveMQtt',
  },
  {
    icon: '2',
    title: 'RawClient MQTT Client',
    audience: 'Best for performance-critical scenarios',
    description:
      'Use the low-level RawClient API for minimal overhead, direct packet handling, and custom message routing.',
    href: '/docs/rawclient/intro',
    cta: 'Explore RawClient',
  },
  {
    icon: '3',
    title: 'Sparkplug Client',
    audience: 'Best for industrial Sparkplug B 3.0',
    description:
      'Build Host Applications and Edge Nodes with Sparkplug topic/payload support powered by HiveMQtt.Sparkplug.',
    href: '/docs/sparkplug/intro',
    cta: 'Explore Sparkplug',
  },
];

function HeroSection() {
  return (
    <header className={styles.hero}>
      <div className={styles.heroBackground}></div>
      <div className="container">
        <div className={styles.heroContent}>
          <div className={styles.heroLogos}>
            <img src="img/logo.png" alt="HiveMQ" className={styles.heroLogo} />
          </div>
          <Heading as="h1" className={styles.heroTitle}>
            HiveMQtt
          </Heading>
          <p className={styles.heroSubtitle}>
            HiveMQ MQTT & Sparkplug Clients for C#/.NET
          </p>
          <p className={styles.heroDescription}>
            Build reliable, high-performance IoT applications with HiveMQ's official C#/.NET client lineup:
            HiveMQtt, RawClient, and HiveMQtt.Sparkplug.
          </p>
          <div className={styles.heroBadges}>
            <img src="https://img.shields.io/nuget/v/HiveMQtt?style=for-the-badge&color=ffc107" alt="NuGet Version" />
            <img src="https://img.shields.io/nuget/dt/HiveMQtt?style=for-the-badge&color=28a745" alt="NuGet Downloads" />
            <img src="https://img.shields.io/github/license/hivemq/hivemq-mqtt-client-dotnet?style=for-the-badge&color=0d6efd" alt="License" />
          </div>
          <div className={styles.heroButtons}>
            <Link className="button button--primary button--lg" to="/docs/hivemqtt/quickstart">
              Get Started
            </Link>
            <Link className="button button--secondary button--lg" to="https://github.com/hivemq/hivemq-mqtt-client-dotnet">
              View on GitHub
            </Link>
          </div>
        </div>
      </div>
    </header>
  );
}

function InstallSection() {
  return (
    <section className={styles.installSection}>
      <div className="container">
        <div className={styles.installCard}>
          <div className={styles.installHeader}>
            <span className={styles.installIcon}>📦</span>
            <Heading as="h2">Install in Seconds</Heading>
          </div>
          <div className={styles.installCode}>
            <CodeBlock language="bash">{installCode}</CodeBlock>
          </div>
          <p className={styles.installNote}>
            Available on <a href="https://www.nuget.org/packages/HiveMQtt" target="_blank" rel="noopener noreferrer">NuGet</a> • 
            Supports .NET 6, 7, 8, 9, and 10
          </p>
        </div>
      </div>
    </section>
  );
}

function FeaturesSection() {
  return (
    <section className={styles.featuresSection}>
      <div className="container">
        <div className={styles.sectionHeader}>
          <Heading as="h2">Why Choose HiveMQtt?</Heading>
          <p>Built by the MQTT experts at HiveMQ for production-grade IoT applications</p>
        </div>
        <div className={styles.featuresGrid}>
          {FeatureList.map((feature, idx) => (
            <div key={idx} className={styles.featureCard}>
              <span className={styles.featureIcon}>{feature.icon}</span>
              <Heading as="h3">{feature.title}</Heading>
              <p>{feature.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

function ClientOptionsSection() {
  return (
    <section className={styles.clientOptionsSection}>
      <div className="container">
        <div className={styles.sectionHeader}>
          <Heading as="h2">Choose Your Client Path</Heading>
          <p>Start with the client that matches your use case, then dive into targeted docs.</p>
        </div>
        <div className={styles.clientOptionsGrid}>
          {ClientOptionsList.map((client) => (
            <div key={client.title} className={styles.clientOptionCard}>
              <span className={styles.clientOptionBadge}>{client.icon}</span>
              <Heading as="h3">{client.title}</Heading>
              <p className={styles.clientAudience}>{client.audience}</p>
              <p>{client.description}</p>
              <Link className="button button--primary" to={client.href}>
                {client.cta}
              </Link>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

function CodeExampleSection() {
  return (
    <section className={styles.codeSection}>
      <div className="container">
        <div className="row">
          <div className="col col--5">
            <div className={styles.codeDescription}>
              <Heading as="h2">Simple, Intuitive API</Heading>
              <p>
                Get connected and start publishing messages in minutes. The HiveMQtt client 
                features a clean, builder-based API that makes MQTT integration straightforward.
              </p>
              <ul className={styles.codeFeatures}>
                <li>✓ Async/await pattern throughout</li>
                <li>✓ Fluent builder configuration</li>
                <li>✓ Event-driven message handling</li>
                <li>✓ Automatic reconnection support</li>
                <li>✓ Per-subscription handlers</li>
              </ul>
              <Link className="button button--primary" to="/docs/hivemqtt/quickstart">
                View Full Documentation
              </Link>
            </div>
          </div>
          <div className="col col--7">
            <div className={styles.codeBlock}>
              <CodeBlock language="csharp" title="QuickStart.cs">
                {quickStartCode}
              </CodeBlock>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

function UseCasesSection() {
  return (
    <section className={styles.useCasesSection}>
      <div className="container">
        <div className={styles.sectionHeader}>
          <Heading as="h2">Built for Real-World IoT</Heading>
          <p>From edge devices to enterprise systems, HiveMQtt powers mission-critical applications</p>
        </div>
        <div className={styles.useCasesGrid}>
          {UseCaseList.map((useCase, idx) => (
            <div key={idx} className={styles.useCaseCard}>
              <span className={styles.useCaseIcon}>{useCase.icon}</span>
              <Heading as="h4">{useCase.title}</Heading>
              <p>{useCase.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

function HiveMQSection() {
  return (
    <section className={styles.hivemqSection}>
      <div className="container">
        <div className={styles.hivemqContent}>
          <div className={styles.hivemqText}>
            <Heading as="h2">Powered by HiveMQ</Heading>
            <p className={styles.hivemqDescription}>
              HiveMQ is the enterprise MQTT platform trusted by leading organizations worldwide. 
              With 99.999% uptime, support for millions of concurrent connections, and seamless 
              integration with enterprise systems, HiveMQ is the foundation for your IoT success.
            </p>
            <div className={styles.hivemqFeatures}>
              <div className={styles.hivemqFeature}>
                <strong>200M+</strong>
                <span>Concurrent Connections</span>
              </div>
              <div className={styles.hivemqFeature}>
                <strong>1M+</strong>
                <span>Messages/Second</span>
              </div>
              <div className={styles.hivemqFeature}>
                <strong>99.999%</strong>
                <span>Uptime SLA</span>
              </div>
            </div>
            <div className={styles.hivemqButtons}>
              <Link className="button button--primary button--lg" to="https://www.hivemq.com/mqtt-cloud-broker/">
                Try HiveMQ Cloud Free
              </Link>
              <Link className="button button--outline button--lg" to="https://www.hivemq.com/">
                Learn More About HiveMQ
              </Link>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

function ResourcesSection() {
  return (
    <section className={styles.resourcesSection}>
      <div className="container">
        <div className={styles.sectionHeader}>
          <Heading as="h2">Resources & Community</Heading>
          <p>Everything you need to succeed with MQTT and HiveMQ</p>
        </div>
        <div className={styles.resourcesGrid}>
          <a href="https://www.hivemq.com/mqtt-essentials/" className={styles.resourceCard} target="_blank" rel="noopener noreferrer">
            <span className={styles.resourceIcon}>📚</span>
            <Heading as="h4">MQTT Essentials</Heading>
            <p>Learn MQTT from scratch with our comprehensive guide</p>
          </a>
          <a href="https://community.hivemq.com/" className={styles.resourceCard} target="_blank" rel="noopener noreferrer">
            <span className={styles.resourceIcon}>💬</span>
            <Heading as="h4">Community Forum</Heading>
            <p>Get help and connect with other developers</p>
          </a>
          <a href="https://github.com/hivemq/hivemq-mqtt-client-dotnet/tree/main/Examples" className={styles.resourceCard} target="_blank" rel="noopener noreferrer">
            <span className={styles.resourceIcon}>💡</span>
            <Heading as="h4">Code Examples</Heading>
            <p>Ready-to-run examples for common use cases</p>
          </a>
          <a href="https://www.hivemq.com/mqtt-toolbox/" className={styles.resourceCard} target="_blank" rel="noopener noreferrer">
            <span className={styles.resourceIcon}>🧰</span>
            <Heading as="h4">MQTT Toolbox</Heading>
            <p>Free tools for MQTT development and testing</p>
          </a>
        </div>
      </div>
    </section>
  );
}

function CTASection() {
  return (
    <section className={styles.ctaSection}>
      <div className="container">
        <div className={styles.ctaContent}>
          <Heading as="h2">Ready to Build?</Heading>
          <p>
            Start building your IoT application today with HiveMQtt and HiveMQ Cloud.
            <br />Free tier includes up to 100 device connections — no credit card required.
          </p>
          <div className={styles.ctaButtons}>
            <Link className="button button--primary button--lg" to="/docs/hivemqtt/quickstart">
              Read the Docs
            </Link>
            <Link className="button button--secondary button--lg" to="https://console.hivemq.cloud/">
              Get HiveMQ Cloud
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}

export default function Home() {
  return (
    <Layout
      title="HiveMQ MQTT & Sparkplug Clients for C#/.NET"
      description="Official HiveMQ C#/.NET clients for MQTT 5.0 and Sparkplug: HiveMQtt, RawClient, and HiveMQtt.Sparkplug.">
      <HeroSection />
      <main>
        <InstallSection />
        <ClientOptionsSection />
        <FeaturesSection />
        <CodeExampleSection />
        <UseCasesSection />
        <HiveMQSection />
        <ResourcesSection />
        <CTASection />
      </main>
    </Layout>
  );
}
