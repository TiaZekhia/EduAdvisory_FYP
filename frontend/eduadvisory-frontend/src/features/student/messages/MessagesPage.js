import { useEffect, useState } from "react";
import { Skeleton } from "primereact/skeleton";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { studentMessagesApi } from "../../../services/students/studentMessagesApi";
import { PageHero } from "../../../shared/components/PageHero";

import DashboardStatCard from "../../../shared/components/DashboardStatCard";
import PageSectionCard from "../../../shared/components/PageSectionCard";
import EmptyStateCard from "../../../shared/components/EmptyStateCard";
import AdvisorContactCard from "../../../shared/components/AdvisorContactCard";

import "./MessagesPage.css";
import MessageCard from "../../../shared/components/MessageCard";

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
          <div className="col-12 col-md-4"><Skeleton height="9rem" borderRadius="20px" /></div>
          <div className="col-12 col-md-4"><Skeleton height="9rem" borderRadius="20px" /></div>
          <div className="col-12 col-md-4"><Skeleton height="9rem" borderRadius="20px" /></div>
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
        <DashboardStatCard
          title="Total Messages"
          value={stats.totalMessages}
          icon="pi pi-envelope"
        />
        <DashboardStatCard
          title="Recent (7 days)"
          value={stats.recent7Days}
          icon="pi pi-clock"
        />
        <DashboardStatCard
          title="This Month"
          value={stats.thisMonth}
          icon="pi pi-calendar"
        />
      </div>

      <PageSectionCard
        title="All Messages"
        subtitle="Broadcast messages and announcements from your advisor"
        className="mb-4"
      >
        {messages.length === 0 ? (
          <EmptyStateCard title="No messages yet" icon="pi pi-inbox" />
        ) : (
          <div className="d-flex flex-column gap-4">
            {messages.map((msg) => (
              <MessageCard key={msg.announcementId} message={msg} />
            ))}
          </div>
        )}
      </PageSectionCard>

      <PageSectionCard title="Need Help?">
        <AdvisorContactCard
          subtitle="Contact your advisor for questions or concerns"
          intro="If you have questions about any announcement or need clarification, you can contact your advisor:"
          advisor={advisor}
          showOffice={true}
        />
      </PageSectionCard>
    </div>
  );
}