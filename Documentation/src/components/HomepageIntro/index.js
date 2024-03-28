import Heading from '@theme/Heading';
import clsx from 'clsx';
import styles from './styles.module.css';

export default function HomepageIntro() {
  return (
    <section className={styles.intro}>
      <div className="container">
        <div className="row">
          <p> This .NET MQTT client was put together with love from the HiveMQ team.  It is currently in a mature BETA state. While it's mostly stable, it is still under development as we add new features.  We'd appreciate any feedback you have. Happy MQTT adventures!  </p>
        </div>
      </div>
    </section>
  );
}
