import { useCallback, useEffect, useRef, useState } from "react";
import { getBroadcasts, markBroadcastAsRead } from "../api/broadcastApi";

const API_ORIGIN = "http://localhost:5267";

export default function BroadcastInbox({ token, liveBroadcast, isOpen, onClose, onUnreadChange }) {
  const [broadcasts, setBroadcasts] = useState([]);
  const onUnreadChangeRef = useRef(onUnreadChange);

  useEffect(() => {
    onUnreadChangeRef.current = onUnreadChange;
  }, [onUnreadChange]);

  const notifyUnread = useCallback((list) => {
    onUnreadChangeRef.current?.(list.filter((b) => !b.isRead).length);
  }, []);
  
  useEffect(() => {
    if (!token) return;
    getBroadcasts(token)
      .then((data) => {
        setBroadcasts(data);
        notifyUnread(data);
      })
      .catch(console.error);
  }, [token, notifyUnread]);

  useEffect(() => {
    if (!liveBroadcast) return;
    setBroadcasts((prev) => {
      const exists = prev.find(
        (b) => b.broadcastMessageId === liveBroadcast.broadcastMessageId
      );

      if (exists) {
        return prev.map((b) =>
          b.broadcastMessageId === liveBroadcast.broadcastMessageId
            ? liveBroadcast
            : b
        );
      }

      const next = [liveBroadcast, ...prev];
      notifyUnread(next);
      return next;
    });
  }, [liveBroadcast, notifyUnread]);

  function getFileUrl(fileUrl) {
    if (!fileUrl) return "#";
    if (fileUrl.startsWith("http")) return fileUrl;
    return `${API_ORIGIN}${fileUrl}`;
  }

  async function handleMarkRead(broadcast) {
    try {
      await markBroadcastAsRead(token, broadcast.broadcastMessageId);
      setBroadcasts((prev) => {
        const next = prev.map((b) =>
          b.broadcastMessageId === broadcast.broadcastMessageId
            ? { ...b, isRead: true, readAt: new Date().toISOString() }
            : b
        );
        onUnreadChange?.(next.filter((b) => !b.isRead).length);
        return next;
      });
    } catch (err) {
      console.error(err);
    }
  }

  return (
    <div className={`broadcast-drawer ${isOpen ? "broadcast-drawer--open" : ""}`}>
      <div className="broadcast-drawer__panel">
        <div className="broadcast-inbox__head">
          <span>Broadcasts</span>
          <button className="broadcast-drawer__close" onClick={onClose} aria-label="Close">
            ✕
          </button>
        </div>

        <div className="broadcast-inbox__list">
          {broadcasts.length === 0 && (
            <span style={{ fontSize: 12, color: "var(--gray-400)" }}>No broadcasts yet.</span>
          )}

          {broadcasts.map((broadcast) => {
            const attachments = broadcast.attachments || [];

            return (
              <div
                key={broadcast.broadcastMessageId}
                className={`bc-card ${!broadcast.isRead ? "bc-card--unread" : ""}`}
              >
                <div className="bc-card__header">
                  <span className="bc-card__title">{broadcast.title}</span>
                  {!broadcast.isRead && <span className="badge badge--new">New</span>}
                </div>

                <div className="bc-card__meta">
                  From {broadcast.advisorName} · {new Date(broadcast.createdAt).toLocaleString()}
                </div>

                <div className="bc-card__content">{broadcast.content}</div>

                {attachments.length > 0 && (
                  <div className="bc-attachments">
                    {attachments.map((file) => {
                      const url = getFileUrl(file.fileUrl);
                      const isImage = file.contentType?.startsWith("image/");

                      return isImage ? (
                        <a
                          key={file.attachmentId}
                          href={url}
                          target="_blank"
                          rel="noreferrer"
                          className="bc-attachment-image-link"
                        >
                          <img
                            src={url}
                            alt={file.fileName}
                            className="bc-attachment-image"
                          />
                        </a>
                      ) : (
                        <a
                          key={file.attachmentId}
                          href={url}
                          target="_blank"
                          rel="noreferrer"
                          className="bc-attachment-file"
                        >
                          <span className="bc-attachment-file__icon">📄</span>
                          <span className="bc-attachment-file__name">{file.fileName}</span>
                        </a>
                      );
                    })}
                  </div>
                )}

                {!broadcast.isRead && (
                  <button
                    className="msg-btn msg-btn--outline msg-btn--sm"
                    onClick={() => handleMarkRead(broadcast)}
                  >
                    Mark as read
                  </button>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}