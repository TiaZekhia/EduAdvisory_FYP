export default function SectionHeader({ title, subtitle, right = null }) {
  return (
    <div className="section-head">
      <div>
        <h4 className="m-0">{title}</h4>
        {subtitle ? <div className="text-muted small">{subtitle}</div> : null}
      </div>
      {right}
    </div>
  );
}