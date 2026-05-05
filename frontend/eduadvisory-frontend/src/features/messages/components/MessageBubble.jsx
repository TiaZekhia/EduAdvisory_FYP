const FILE_BASE_URL = "http://localhost:5267";

function isImage(contentType) {
  return contentType?.startsWith("image/");
}

function getFileUrl(fileUrl) {
  if (!fileUrl) return "#";

  if (fileUrl.startsWith("http")) return fileUrl;

  return `${FILE_BASE_URL}${fileUrl}`;
}

export default function MessageBubble({ message }) {
  const isMine = message.isMine;
  const attachments = message.attachments || [];

  return (
    <div className={`bubble ${isMine ? "bubble--mine" : "bubble--theirs"}`}>
      {message.content && <div>{message.content}</div>}

      {attachments.length > 0 && (
        <div className="bubble__attachments">
          {attachments.map((file) => {
            const url = getFileUrl(file.fileUrl);

            if (isImage(file.contentType)) {
              return (
                <a
                  key={file.attachmentId}
                  href={url}
                  target="_blank"
                  rel="noreferrer"
                  className="bubble__image-link"
                >
                  <img
                    src={url}
                    alt={file.fileName}
                    className="bubble__image"
                  />
                </a>
              );
            }

            return (
              <a
                key={file.attachmentId}
                href={url}
                target="_blank"
                rel="noreferrer"
                className="bubble__file"
              >
                📄 {file.fileName}
              </a>
            );
          })}
        </div>
      )}

      <div className="bubble__time">
  {new Date(message.sentAt).toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  })}

  {message.isMine && (
    <span className={`bubble__ticks ${message.isRead ? "bubble__ticks--read" : ""}`}>
      {message.isRead ? "✓✓" : "✓"}
    </span>
  )}
</div>
    </div>
  );
}