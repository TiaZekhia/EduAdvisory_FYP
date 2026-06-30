import React, { createContext, useContext, useState, useCallback, useEffect, useRef } from "react";
import { layoutApi } from "../../../services/layoutApi";
import { useAuth } from "../../../app/providers/AuthProvider";
import { startChatConnection } from "../signalr/chatConnection";

const MessagesContext = createContext();

export function MessagesProvider({ children }) {
  const [unreadMessagesCount, setUnreadMessagesCount] = useState(0);
  const [isOnMessagesPage, setIsOnMessagesPage] = useState(false);
  const isOnMessagesPageRef = useRef(false);
  const { keycloak } = useAuth();

  // Keep ref in sync so SignalR handler always reads the current value
  useEffect(() => {
    isOnMessagesPageRef.current = isOnMessagesPage;
  }, [isOnMessagesPage]);

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
    if (!isOnMessagesPageRef.current) {
      setUnreadMessagesCount((prev) => prev + 1);
    }
  }, []);

  // Initial fetch on mount
  useEffect(() => {
    if (keycloak?.token) {
      fetchUnreadCount();
    }
  }, [keycloak?.token, fetchUnreadCount]);

  // Set up SignalR — only ReceiveMessage increments the badge (not MessageSent)
  useEffect(() => {
    if (!keycloak?.token) return;

    let mounted = true;

    const setupSignalR = async () => {
      try {
        const connection = await startChatConnection(keycloak.token);
        if (!mounted) return;

        const handleReceiveMessage = () => {
          if (mounted) incrementUnreadCount();
        };

        // Remove any previous provider listener before adding to avoid stacking
        connection.off("ReceiveMessage", handleReceiveMessage);
        connection.on("ReceiveMessage", handleReceiveMessage);
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
