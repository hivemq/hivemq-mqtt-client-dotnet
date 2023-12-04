import Heading from '@theme/Heading';
import clsx from 'clsx';
import styles from './styles.module.css';

export default function HomepageIntro() {
  return (
    <section className={styles.intro}>
      <div className="container">
        <div className="row">
          This .NET MQTT client was put together with love from the HiveMQ team but is still in BETA. As such some things may not work completely until it matures and although unlikely, APIs may change slightly before version 1.0.

          We'd appreciate any feedback you have. Happy MQTT adventures!
        </div>
      </div>
    </section>
  );
}
