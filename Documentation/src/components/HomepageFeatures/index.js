import Heading from '@theme/Heading';
import clsx from 'clsx';
import styles from './styles.module.css';

const FeatureList = [
  {
    title: 'Easy to Use',
    image: 'img/easy-to-use.jpg',
    description: (
      <>
        HiveMQ MQTT Client Libraries are designed to simplify the deployment and implementation of MQTT.
      </>
    ),
  },
  {
    title: 'High Quality',
    image: 'img/high-quality.jpg',
    description: (
      <>
        Supercharge your app development cycle by using high-quality, open-sourced MQTT Client Libraries to build IoT and IIoT applications quickly and easily.
      </>
    ),
  },
  {
    title: 'Reliable',
    image: 'img/reliable.jpg',
    description: (
      <>
        Built, tested, and maintained by dedicated, industry-leading IoT experts at HiveMQ. Minimize performance issues and outages by getting them from the source.
      </>
    ),
  },
];

function Feature({image, title, description}) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <img className={styles.featureSvg} role="img" src={image} />
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
