import { useState } from "react";

const FILE_BASE_URL = "http://localhost:5267";

function isImage(contentType) {
  return contentType?.startsWith("image/");
}

function getFileUrl(fileUrl) {
  if (!fileUrl) return "#";
  if (fileUrl.startsWith("http")) return fileUrl;
  return `${FILE_BASE_URL}${fileUrl}`;
}

export default function MessageBubble({ message, onEdit, onDelete }) {
  const isMine = message.isMine;
  const attachments = message.attachments || [];
  const [menuOpen, setMenuOpen] = useState(false);

  const canEdit =
    isMine &&
    !message.isDeleted &&
    attachments.length === 0 &&
    !!message.content;

  const canDelete = isMine && !message.isDeleted;

  return (
    <div className={`bubble ${isMine ? "bubble--mine" : "bubble--theirs"}`}>
      {canDelete && (
        <div className="bubble__menu-wrap">
          <button
            type="button"
            className="bubble__menu-btn"
            onClick={() => setMenuOpen((prev) => !prev)}
          >
            ⋯
          </button>

          {menuOpen && (
            <div className="bubble__menu">
              {canEdit && (
                <button
                  type="button"
                  onClick={() => {
                    setMenuOpen(false);
                    onEdit?.(message);
                  }}
                >
                  <i className="pi pi-pencil" />
                  <span>Edit</span>
                </button>
              )}

              <button
                type="button"
                onClick={() => {
                  setMenuOpen(false);
                  onDelete?.(message);
                }}
              >
                <i className="pi pi-trash" />
                <span>Delete</span>
              </button>
            </div>
          )}
        </div>
      )}

      {message.isDeleted ? (
        <div className="bubble__deleted">This message was deleted</div>
      ) : (
        <>
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
                        onLoad={() => {
                          window.dispatchEvent(new Event("chat-image-loaded"));
                        }}
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
                    <span className="bubble__file-name">
                      📄 {file.fileName}
                    </span>
                  </a>
                );
              })}
            </div>
          )}
        </>
      )}

      <div className="bubble__time">
        {new Date(message.sentAt).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        })}

        {message.editedAt && !message.isDeleted && (
          <span className="bubble__edited">edited</span>
        )}

        {isMine && !message.isDeleted && (
          <span className={`bubble__ticks ${message.isRead ? "bubble__ticks--read" : ""}`}>
            {message.isRead ? "✓✓" : "✓"}
          </span>
        )}
      </div>
    </div>
  );
}