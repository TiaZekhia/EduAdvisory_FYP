import React, { createContext, useContext, useState, useCallback, useEffect } from "react";
import { layoutApi } from "../../../services/layoutApi";
import { useAuth } from "../../../app/providers/AuthProvider";
import { startChatConnection } from "../signalr/chatConnection";

const MessagesContext = createContext();

export function MessagesProvider({ children }) {
  const [unreadMessagesCount, setUnreadMessagesCount] = useState(0);
  const [isOnMessagesPage, setIsOnMessagesPage] = useState(false);
  const { keycloak } = useAuth();

  const fetchUnreadCount = useCallback(async () => {
    try {
      const response = await layoutApi.getUnreadMessagesCount();
      setUnreadMessagesCount(response.data.unreadMessagesCount ?? 0);
    } catch (error) {
      console.error("Failed to fetch unread count:", error);
    }
  }, []);

  const resetUnreadCount = useCallback(() => {
    setUnreadMessagesCount(0);
  }, []);

  const incrementUnreadCount = useCallback(() => {
    // Only increment if NOT on the messages page
    if (!isOnMessagesPage) {
      setUnreadMessagesCount((prev) => prev + 1);
    }
  }, [isOnMessagesPage]);

  // Initial fetch on mount
  useEffect(() => {
    if (keycloak?.token) {
      fetchUnreadCount();
    }
  }, [keycloak?.token, fetchUnreadCount]);

  // Set up SignalR connection to listen for messages in the background
  useEffect(() => {
    if (!keycloak?.token) return;

    let mounted = true;

    const setupSignalR = async () => {
      try {
        const connection = await startChatConnection(keycloak.token);

        if (!mounted) return;

        // Listen for new messages even when not on messages page
        const handleReceiveMessage = (message) => {
          if (mounted) {
            incrementUnreadCount();
          }
        };

        const handleMessageSent = (message) => {
          if (mounted) {
            incrementUnreadCount();
          }
        };

        connection.on("ReceiveMessage", handleReceiveMessage);
        connection.on("MessageSent", handleMessageSent);
      } catch (error) {
        console.error("Failed to setup SignalR:", error);
      }
    };

    setupSignalR();

    return () => {
      mounted = false;
    };
  }, [keycloak?.token, incrementUnreadCount]);

  return (
    <MessagesContext.Provider
      value={{
        unreadMessagesCount,
        setUnreadMessagesCount,
        fetchUnreadCount,
        resetUnreadCount,
        incrementUnreadCount,
        setIsOnMessagesPage,
      }}
    >
      {children}
    </MessagesContext.Provider>
  );
}

export function useMessages() {
  const context = useContext(MessagesContext);
  if (!context) {
    throw new Error("useMessages must be used within MessagesProvider");
  }
  return context;
}
