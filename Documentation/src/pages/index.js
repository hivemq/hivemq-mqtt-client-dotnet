import Heading from '@theme/Heading';
import HomepageFeatures from '@site/src/components/HomepageFeatures';
import HomepageIntro from '@site/src/components/HomepageIntro';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import clsx from 'clsx';
import styles from './index.module.css';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <img src="img/client-logo-two.png" alt="HiveMQtt Logo" className="hero__logo" />
        <img src="img/csharp-logo.png" alt="HiveMQtt Logo" className="hero__logo" />
        <img src="img/dotnet-logo.png" alt="HiveMQtt Logo" className="hero__logo" />
      </div>
    </header>
  );
}

export default function Home() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      description="The HiveMQ C# MQTT client for .NET">
      <HomepageHeader />
      <main>
        <HomepageFeatures />
        <HomepageIntro />
      </main>
    </Layout>
  );
}
