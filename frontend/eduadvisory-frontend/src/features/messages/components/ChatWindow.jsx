import { useEffect, useRef, useState } from "react";
import { Toast } from "primereact/toast";
import MessageBubble from "./MessageBubble";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { editMessage, deleteMessage, sendMessage, sendMessageWithFiles } from "../api/messageApi";

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

export default function ChatWindow({ token, selectedConversation, messages, setMessages }) {
  const [content, setContent] = useState("");
  const [files, setFiles] = useState([]);
  const fileInputRef = useRef(null);
  const toast = useRef(null);
  const messagesRef = useRef(null);
  const [editingMessage, setEditingMessage] = useState(null);

  useEffect(() => {
  const timer = setTimeout(() => {
    if (messagesRef.current) {
      messagesRef.current.scrollTo({
        top: messagesRef.current.scrollHeight,
        behavior: "smooth",
      });
    }
  }, 200);

  return () => clearTimeout(timer);
}, [messages.length, selectedConversation?.conversationId]);


  function showError(message) {
    toast.current?.show({
      severity: "error",
      summary: "File Error",
      detail: message,
      life: 3500,
    });
  }

  function handleFileChange(e) {
    const selectedFiles = Array.from(e.target.files || []);
    const validFiles = [];

    selectedFiles.forEach((file) => {
      if (file.size === 0) {
        showError(`${file.name} is empty and cannot be uploaded.`);
        return;
      }

      if (file.size > MAX_FILE_SIZE) {
        showError(`${file.name} is too large. Maximum size is 10MB.`);
        return;
      }

      validFiles.push(file);
    });

    setFiles((prev) => [...prev, ...validFiles]);
    e.target.value = "";
  }

  function removeFile(index) {
    setFiles((prev) => prev.filter((_, i) => i !== index));
  }

  async function handleSend(e) {
  e.preventDefault();

  if (!selectedConversation) return;

  const text = content.trim();
  const hasFiles = files.length > 0;

  if (!text && !hasFiles) return;

  try {
    if (editingMessage) {
      await editMessage(token, editingMessage.messageId, text);
      setEditingMessage(null);
      setContent("");
      return;
    }

    const filesToSend = files;
    setContent("");
    setFiles([]);

    if (hasFiles) {
      await sendMessageWithFiles(
        token,
        selectedConversation.conversationId,
        text,
        filesToSend
      );
    } else {
      await sendMessage(token, selectedConversation.conversationId, text);
    }
  } catch (err) {
    console.error(err);
    toast.current?.show({
      severity: "error",
      summary: "Message Failed",
      detail: "Could not complete the action.",
      life: 3500,
    });
  }
}
function handleDeleteMessage(message) {
  confirmDialog({
    message: "Delete this message for everyone?",
    header: "Delete message",
    icon: "pi pi-trash",
    acceptLabel: "Yes, delete",
    rejectLabel: "Cancel",
    acceptClassName: "p-button-danger",
    accept: async () => {
      try {
        await deleteMessage(token, message.messageId);
      } catch (err) {
        console.error(err);
        toast.current?.show({
          severity: "error",
          summary: "Delete failed",
          detail: "Could not delete the message.",
          life: 3500,
        });
      }
    },
  });
}
function handleEditMessage(message) {
  setEditingMessage(message);
  setContent(message.content || "");
  setFiles([]);
}

  if (!selectedConversation) {
    return (
      <div className="chat-window">
        <Toast ref={toast} />
        <div className="chat-window__empty">Select a conversation</div>
      </div>
    );
  }

  return (
    <div className="chat-window">
      <Toast ref={toast} />
<ConfirmDialog />
      <div className="chat-window__head">
        {selectedConversation.studentName || selectedConversation.advisorName}
      </div>

<div className="chat-window__messages" ref={messagesRef}>
          {messages.map((message) => (
<MessageBubble
  key={message.messageId}
  message={message}
  token={token}
  onEdit={handleEditMessage}
  onDelete={handleDeleteMessage}
/>      ))}
      </div>

      {files.length > 0 && (
        <div className="file-preview-bar">
          {files.map((file, index) => {
            const isImage = file.type.startsWith("image/");

            return (
              <div className="file-preview-card" key={`${file.name}-${index}`}>
                <button
                  type="button"
                  className="file-preview-card__remove"
                  onClick={() => removeFile(index)}
                >
                  ×
                </button>

                {isImage ? (
                  <img
                    src={URL.createObjectURL(file)}
                    alt={file.name}
                    className="file-preview-card__image"
                  />
                ) : (
                  <div className="file-preview-card__doc">
                    <span className="file-preview-card__icon">📎</span>
                  </div>
                )}

                <div className="file-preview-card__name" title={file.name}>
                  {file.name}
                </div>

                <div className="file-preview-card__size">
                  {(file.size / 1024).toFixed(1)} KB
                </div>
              </div>
            );
          })}
        </div>
      )}
{editingMessage && (
  <div className="chat-edit-banner">
    <span>
      <i className="pi pi-pencil" /> Editing message
    </span>

    <button
      type="button"
      className="chat-edit-banner__cancel"
      onClick={() => {
        setEditingMessage(null);
        setContent("");
      }}
    >
      Cancel
    </button>
  </div>
)}
      <form onSubmit={handleSend} className="chat-window__form">
        <input
          ref={fileInputRef}
          type="file"
          multiple
          hidden
          onChange={handleFileChange}
          accept="image/*,.pdf,.doc,.docx,.xls,.xlsx,.txt"
        />

       <button
  type="button"
  className="msg-btn msg-btn--outline"
  onClick={() => fileInputRef.current?.click()}
  disabled={!!editingMessage}
>
  <i className="pi pi-paperclip" />
</button>

        <input
          className="msg-input"
          style={{ flex: 1 }}
          placeholder="Type your message…"
          value={content}
          onChange={(e) => setContent(e.target.value)}
        />

        <button className="msg-btn" type="submit">
  {editingMessage ? "Save" : "Send"}
</button>
      </form>
    </div>
  );
}