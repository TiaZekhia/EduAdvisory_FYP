export default function MessageCard({ message }) {
  return (
    <div className={`message-card ${message.isStatic ? "message-card-demo" : ""}`}>
      {message.isStatic && <div className="demo-badge">Preview Message</div>}

      <div className="fw-semibold fs-4">{message.title}</div>

      <div className="message-meta mt-3">
        <span className="message-meta-item">
          <i className="pi pi-calendar" />
          {formatDate(message.createdAt)}
        </span>
        <span className="message-meta-item">
          <i className="pi pi-user" />
          From: {message.advisorName}
        </span>
      </div>

      <div className="message-content mt-4">{message.content}</div>

      <hr className="my-4" />

      <div className="text-muted small">
        This message was sent to {message.recipientsCount} students in your
        advising group
      </div>
    </div>
  );
}

function formatDate(dateLike) {
  const d = new Date(dateLike);
  return d.toLocaleDateString(undefined, {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}