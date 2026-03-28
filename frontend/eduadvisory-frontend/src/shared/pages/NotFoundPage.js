import { useNavigate } from "react-router-dom";
import "./NotFoundPage.css";


export default function NotFoundPage() {
  const navigate = useNavigate();
  const role = localStorage.getItem("role");
  
  return (
    <div className="nf-wrapper">
      <div className="nf-content">
        <h1 className="nf-oops">Oops!</h1>

        <h2 className="nf-title">404 - PAGE NOT FOUND</h2>

        <p className="nf-text">
          The page you are looking for might have been removed,
          had its name changed or is temporarily unavailable.
        </p>

        <button
          className="nf-btn"
          onClick={() => role === "ADVISOR" ? navigate("/advisor/dashboard") : navigate("/advisor/dashboard")}
        >
          GO TO HOMEPAGE
        </button>
      </div>
    </div>
  );
}