import { Skeleton } from "primereact/skeleton";
import {
  User,
  Hash,
  Mail,
  BookOpen,
  CalendarDays,
  BarChart2,
  ShieldCheck,
  UserSquare2,
  Building2,
  Clock,
} from "lucide-react";
import { PageHero } from "../../../shared/components/PageHero";
import { useStudentProfile } from "./hooks/useStudentProfile";
import "./StudentProfilePage.css";

function ProfileField({ icon: Icon, label, children }) {
  return (
    <div className="profile-field">
      <label className="profile-label">
        <span className="profile-label-icon">
          <Icon size={13} strokeWidth={2.2} />
        </span>
        {label}
      </label>
      <div className="profile-value">{children}</div>
    </div>
  );
}

export default function StudentProfilePage() {
  const { profile, loading, error } = useStudentProfile();

  if (loading) {
    return (
      <div className="student-profile-page">
        <PageHero title="Profile" subtitle="View your student profile information" />
        <div className="container-fluid">
          <div className="row g-4">
            <div className="col-12 col-lg-8">
              <div className="profile-card">
                <Skeleton height="2.5rem" width="60%" className="mb-3" />
                <Skeleton height="1rem" width="80%" className="mb-2" />
                <Skeleton height="1rem" width="75%" className="mb-2" />
                <Skeleton height="1rem" width="70%" className="mb-4" />
                <Skeleton height="1rem" width="100%" className="mb-2" />
                <Skeleton height="1rem" width="100%" className="mb-2" />
                <Skeleton height="1rem" width="100%" />
              </div>
            </div>
            <div className="col-12 col-lg-4">
              <div className="profile-card">
                <Skeleton height="2rem" width="80%" className="mb-3" />
                <Skeleton height="1rem" width="100%" className="mb-2" />
                <Skeleton height="1rem" width="100%" className="mb-2" />
                <Skeleton height="1rem" width="100%" />
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="student-profile-page">
        <PageHero title="Profile" subtitle="View your student profile information" />
        <div className="container-fluid">
          <div className="alert alert-danger" role="alert">{error}</div>
        </div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="student-profile-page">
        <PageHero title="Profile" subtitle="View your student profile information" />
        <div className="container-fluid">
          <div className="alert alert-info" role="alert">No profile information found.</div>
        </div>
      </div>
    );
  }

  return (
    <div className="student-profile-page">
      <PageHero title="Profile" subtitle="View your student profile information" />
      <div className="container-fluid">
        <div className="row g-4">

          {/* Main Profile Section */}
          <div className="col-12 col-lg-8">
            <div className="profile-card">
              <h3 className="profile-section-title">Personal Information</h3>
              <div className="profile-info-grid">
                <ProfileField icon={User} label="Full Name">
                  {profile?.fullName || "-"}
                </ProfileField>
                <ProfileField icon={Hash} label="Student ID">
                  {profile?.studentId || "-"}
                </ProfileField>
                <ProfileField icon={Mail} label="Email">
                  <span className="profile-value-email">{profile?.email || "-"}</span>
                </ProfileField>
                <ProfileField icon={BookOpen} label="Program Code">
                  {profile?.programCode || "-"}
                </ProfileField>
              </div>

              <h3 className="profile-section-title mt-5">Academic Information</h3>
              <div className="profile-info-grid">
                <ProfileField icon={CalendarDays} label="Current Semester">
                  {profile?.currentSemester || "-"}
                </ProfileField>
                <ProfileField icon={BarChart2} label="Current GPA">
                  {profile?.currentGpa?.toFixed(2) || "-"}
                </ProfileField>
                <ProfileField icon={ShieldCheck} label="Academic Status">
                  <span
                    className={`status-badge status-${
                      profile?.academicStatus?.toLowerCase() || "unknown"
                    }`}
                  >
                    {profile?.academicStatus || "-"}
                  </span>
                </ProfileField>
              </div>
            </div>
          </div>

          {/* Advisor Section */}
          <div className="col-12 col-lg-4">
            <div className="profile-card">
              <h3 className="profile-section-title">Your Advisor</h3>
              <div className="profile-info-stack">
                <ProfileField icon={UserSquare2} label="Advisor Name">
                  {profile?.advisorName || "-"}
                </ProfileField>
                <ProfileField icon={Mail} label="Email">
                  <span className="profile-value-email">{profile?.advisorEmail || "-"}</span>
                </ProfileField>
                <ProfileField icon={Building2} label="Office">
                  {profile?.advisorOffice || "-"}
                </ProfileField>
                <ProfileField icon={Clock} label="Office Hours">
                  {profile?.advisorOfficeHours || "-"}
                </ProfileField>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}