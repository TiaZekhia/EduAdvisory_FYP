export default function MessageBubble({ message }) {
  const isMine = message.isMine;

  return (
    <div className={`bubble ${isMine ? "bubble--mine" : "bubble--theirs"}`}>
      <div>{message.content}</div>
      <div className="bubble__time">
        {new Date(message.sentAt).toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        })}
      </div>
    </div>
  );
}