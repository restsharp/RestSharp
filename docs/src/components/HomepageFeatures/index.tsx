import clsx from "clsx";
import Heading from "@theme/Heading";
import styles from "./styles.module.css";

type FeatureItem = {
  title: string;
  // Svg: React.ComponentType<React.ComponentProps<'svg'>>;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: "Serialization",
    // Svg: require('@site/static/img/undraw_docusaurus_mountain.svg').default,
    description: (
      <>
        Make calls using XML or JSON body, and receive XML or JSON responses.
        RestSharp takes care of serializing requests and deserializing responses, as well as adding the correct content type.
      </>
    ),
  },
  {
    title: "Fully async",
    // Svg: require('@site/static/img/undraw_docusaurus_tree.svg').default,
    description: (
      <>
          RestSharp API has an extensive number of async functions to make all sort of HTTP calls.
          It still provides sync overloads to allow using RestSharp in legacy applications or non-async environments.
      </>
    ),
  },
  {
    title: "Parameters",
    // Svg: require('@site/static/img/undraw_docusaurus_react.svg').default,
    description: (
      <>
          Whether you want to add a query, a URL, or URL-encoded form parameters, RestSharp allows doing it with one line of code.
          The same applies to sending files and using multipart forms.
      </>
    ),
  },
];

function Feature({ title, description }: FeatureItem) {
  return (
    <div className={clsx("col col--4")}>
      {/*<div className="text--center">*/}
      {/*  <Svg className={styles.featureSvg} role="img" />*/}
      {/*</div>*/}
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): JSX.Element {
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
