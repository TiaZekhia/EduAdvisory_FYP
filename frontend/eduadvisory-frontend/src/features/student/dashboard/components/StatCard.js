import { Card } from "primereact/card";

export default function StatCard({ title, value, subtitle }) {
  return (
    <Card className="h-100 shadow-sm">
      <div className="text-muted small">{title}</div>
      <div className="fs-2 fw-semibold mt-2">{value}</div>
      <div className="text-muted small mt-1">{subtitle}</div>
    </Card>
  );
}