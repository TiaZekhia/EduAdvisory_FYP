export default function ConversationList({
  conversations,
  selectedConversationId,
  onSelectConversation,
  role,
}) {
  return (
    <div className="msg-sidebar">
      <div className="msg-sidebar__head">
        <div className="msg-sidebar__title">Messages</div>
      </div>

      <div className="msg-sidebar__list">
        {conversations.length === 0 && (
          <div style={{ padding: "12px 16px", fontSize: 12, color: "var(--gray-400)" }}>
            No conversations yet.
          </div>
        )}

        {conversations.map((conversation) => {
          const displayName =
            role === "student" ? conversation.advisorName : conversation.studentName;

          return (
            <button
              key={conversation.conversationId}
              className={`sidebar-item ${
                selectedConversationId === conversation.conversationId
                  ? "sidebar-item--active"
                  : ""
              }`}
              onClick={() => onSelectConversation(conversation)}
            >
              <div className="sidebar-item__row">
                <span className="sidebar-item__name">{displayName}</span>
                {conversation.unreadCount > 0 && (
                  <span className="badge">{conversation.unreadCount}</span>
                )}
              </div>
              <span className="sidebar-item__preview">
                {conversation.lastMessage || "No messages yet"}
              </span>
            </button>
          );
        })}
      </div>
    </div>
  );
}