import { useEffect, useState } from "react";
import { Card } from "primereact/card";
import { Skeleton } from "primereact/skeleton";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { studentMessagesApi } from "../../../services/students/studentMessagesApi";
import "./MessagesPage.css";
import { PageHero } from "../../../shared/components/PageHero";

export default function MessagesPage() {
  const { summary } = useStudentSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const [stats, setStats] = useState({
    totalMessages: 0,
    recent7Days: 0,
    thisMonth: 0,
  });

  const [messages, setMessages] = useState([]);
  const [advisor, setAdvisor] = useState(null);

  useEffect(() => {
    let mounted = true;

    setLoading(true);
    setErr("");

    Promise.all([
      studentMessagesApi.getSummary(),
      studentMessagesApi.getMessages(20),
      studentMessagesApi.getAdvisor(),
    ])
      .then(([summaryRes, messagesRes, advisorRes]) => {
        if (!mounted) return;

        setStats(
          summaryRes?.data ?? {
            totalMessages: 0,
            recent7Days: 0,
            thisMonth: 0,
          }
        );

        setMessages(messagesRes?.data ?? []);
        setAdvisor(advisorRes?.data ?? null);
      })
      .catch((e) => {
        console.error("Messages load error:", e);
        if (!mounted) return;
        setErr(e?.response?.data ?? e?.message ?? "Failed to load messages");
      })
      .finally(() => {
        if (!mounted) return;
        setLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, []);

  if (loading) {
    return (
      <div className="messages-page container-fluid p-4 p-lg-5">
        <div className="mb-4">
          <Skeleton width="14rem" height="2.2rem" className="mb-2" />
          <Skeleton width="24rem" height="1.2rem" />
        </div>
        <div className="row g-4 mb-4">
          <div className="col-12 col-md-4">
            <Skeleton height="9rem" borderRadius="20px" />
          </div>
          <div className="col-12 col-md-4">
            <Skeleton height="9rem" borderRadius="20px" />
          </div>
          <div className="col-12 col-md-4">
            <Skeleton height="9rem" borderRadius="20px" />
          </div>
        </div>
        <Skeleton height="24rem" className="mb-4" borderRadius="24px" />
        <Skeleton height="14rem" borderRadius="24px" />
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;

  return (
    <div className="messages-page container-fluid p-3 p-md-4">
      <PageHero
              title="Messages"
              badge={`${summary?.programCode} • Semester ${summary?.currentSemester}`}
              subtitle="Important announcements and updates from your academic advisor"
      />

      <div className="row g-4 mb-4">
        <StatCard
          title="Total Messages"
          value={stats.totalMessages}
          icon="pi pi-envelope"
        />
        <StatCard
          title="Recent (7 days)"
          value={stats.recent7Days}
          icon="pi pi-clock"
        />
        <StatCard
          title="This Month"
          value={stats.thisMonth}
          icon="pi pi-calendar"
        />
      </div>

      <Card className="messages-card shadow-sm border-0 mb-4">
        <div className="messages-card-header">
          <div>
            <div className="fw-semibold fs-4">All Messages</div>
            <div className="text-muted mt-1">
              Broadcast messages and announcements from your advisor
            </div>
          </div>
        </div>

        {messages.length === 0 && (
          <div className="empty-state mb-4">
            <div className="empty-icon">
              <i className="pi pi-inbox" />
            </div>
            <div>
              <div className="empty-title">No messages yet</div>
            </div>
          </div>
        )}

        <div className="d-flex flex-column gap-4">
          {messages.map((msg) => (
            <MessageCard key={msg.announcementId} message={msg} />
          ))}
        </div>
      </Card>

      <Card className="messages-card shadow-sm border-0">
        <div className="fw-semibold fs-4">Need Help?</div>
        <div className="text-muted mb-3">
          Contact your advisor for questions or concerns
        </div>

        <div className="help-text mb-3">
          If you have questions about any announcement or need clarification,
          you can contact your advisor:
        </div>

        <div className="advisor-box">
          <div className="advisor-name">{advisor?.name ?? "Not assigned"}</div>
          <div className="advisor-item">
            <i className="pi pi-envelope me-2" />
             <span> Email: </span>
            {advisor?.email ?? "-"}
          </div>
          <div className="advisor-item">
            <i className="pi pi-building me-2" />
            <span> Office: </span>
            {advisor?.office ?? "Building A, Room 305"}
          </div>
          <div className="advisor-item">
            <i className="pi pi-clock me-2" />
            <span> Office Hours: </span>
            {advisor?.officeHours ?? "Mon, Wed, Fri: 2-4 PM"}
          </div>
        </div>
      </Card>
    </div>
  );
}

function StatCard({ title, value, icon }) {
  return (
    <div className="col-12 col-md-4">
      <div className="stat-card h-100">
        <div className="stat-card-top">
          <div className="stat-icon">{icon ? <i className={icon} /> : null}</div>
          <div className="stat-title">{title}</div>
        </div>
        <div className="stat-value">{value}</div>
      </div>
    </div>
  );
}

function MessageCard({ message }) {
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