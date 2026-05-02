import { useEffect, useState } from "react";
import { getBroadcasts, markBroadcastAsRead } from "../api/broadcastApi";

export default function BroadcastInbox({ token, liveBroadcast, isOpen, onClose, onUnreadChange }) {
  const [broadcasts, setBroadcasts] = useState([]);

  // Load initial broadcasts and report unread count to parent
  useEffect(() => {
    if (!token) return;
    getBroadcasts(token)
      .then((data) => {
        setBroadcasts(data);
        onUnreadChange?.(data.filter((b) => !b.isRead).length);
      })
      .catch(console.error);
  }, [token]);

  // Append live broadcast and bump the count
  useEffect(() => {
    if (!liveBroadcast) return;
    setBroadcasts((prev) => {
      if (prev.some((b) => b.broadcastMessageId === liveBroadcast.broadcastMessageId))
        return prev;
      const next = [liveBroadcast, ...prev];
      onUnreadChange?.(next.filter((b) => !b.isRead).length);
      return next;
    });
  }, [liveBroadcast]);

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

          {broadcasts.map((broadcast) => (
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

              {!broadcast.isRead && (
                <button
                  className="msg-btn msg-btn--outline msg-btn--sm"
                  onClick={() => handleMarkRead(broadcast)}
                >
                  Mark as read
                </button>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}