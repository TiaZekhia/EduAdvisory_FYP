import { ProgressSpinner } from "primereact/progressspinner";
import "./Loading.css";

export  function Loading({ text = "Loading..." }) {
  return (
    <div className="loading-page">
      <ProgressSpinner />
      <div className="loading-text">{text}</div>
    </div>
  );
}