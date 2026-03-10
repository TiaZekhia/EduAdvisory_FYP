import { ProgressBar } from "primereact/progressbar";

export default function ProgressSummaryCard({
  title,
  subtitle,
  current,
  total,
  percent,
  stats,
}) {
  return (
    <div className="card shadow-sm border-0">
      <div className="card-body">
        <h5 className="card-title mb-1">{title}</h5>
        <div className="text-muted small mb-3">{subtitle}</div>

        <div className="d-flex justify-content-between mb-2">
          <div className="text-muted small">Credits Completed</div>
          <div className="text-muted small">
            {current} / {total}
          </div>
        </div>

        <ProgressBar
          value={percent}
          showValue={false}
          className="deg-progressbar"
        />

        <div className="row text-center mt-3">
          <div className="col">
            <div className="fs-4 fw-semibold text-success">{stats.passed}</div>
            <div className="text-muted small">Passed</div>
          </div>
          <div className="col">
            <div className="fs-4 fw-semibold text-primary">{stats.inProgress}</div>
            <div className="text-muted small">In Progress</div>
          </div>
          <div className="col">
            <div className="fs-4 fw-semibold text-danger">{stats.failed}</div>
            <div className="text-muted small">Failed</div>
          </div>
        </div>
      </div>
    </div>
  );
}