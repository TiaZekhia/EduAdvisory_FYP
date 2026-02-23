import "./PageHero.css";

export function PageHero({
  title,
  badge,
  subtitle,
  children,
}) {
  return (
    <div className="page-hero">
      <div className="page-hero-left">
        <div className="page-hero-top">
          <h2 className="page-hero-title">{title}</h2>

          {badge && (
            <span className="page-hero-badge">
              {badge}
            </span>
          )}
        </div>

        {subtitle && (
          <h3 className="page-hero-subtitle">
            {subtitle}
          </h3>
        )}

        {children}
      </div>
    </div>
  );
}