import { Skeleton } from "primereact/skeleton";
import {
  User,
  Hash,
  Mail,
  Building2,
  Clock,
  Users,
  GraduationCap,
} from "lucide-react";
import { PageHero } from "../../../shared/components/PageHero";
import { useAdvisorProfile } from "./hooks/useAdvisorProfile";
import "./AdvisorProfilePage.css";

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

export default function AdvisorProfilePage() {
  const { profile, loading, error } = useAdvisorProfile();

  if (loading) {
    return (
      <div className="advisor-profile-page">
        <PageHero title="Profile" subtitle="View your advisor profile information" />
        <div className="container-fluid">
          <div className="row g-4">
            <div className="col-12 col-lg-8">
              <div className="profile-card">
                <Skeleton height="2.5rem" width="60%" className="mb-3" />
                <Skeleton height="1rem" width="80%" className="mb-2" />
                <Skeleton height="1rem" width="75%" className="mb-2" />
                <Skeleton height="1rem" width="70%" className="mb-4" />
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
      <div className="advisor-profile-page">
        <PageHero title="Profile" subtitle="View your advisor profile information" />
        <div className="container-fluid">
          <div className="alert alert-danger" role="alert">{error}</div>
        </div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="advisor-profile-page">
        <PageHero title="Profile" subtitle="View your advisor profile information" />
        <div className="container-fluid">
          <div className="alert alert-info" role="alert">No profile information found.</div>
        </div>
      </div>
    );
  }

  return (
    <div className="advisor-profile-page">
      <PageHero title="Profile" subtitle="View your advisor profile information" />
      <div className="container-fluid">
        <div className="row g-4">

          {/* Main Profile Section */}
          <div className="col-12 col-lg-8">
            <div className="profile-card">
              <h3 className="profile-section-title">Personal Information</h3>
              <div className="profile-info-grid">
                <ProfileField icon={User} label="Name">
                  {profile?.name || "-"}
                </ProfileField>
                <ProfileField icon={Hash} label="Advisor ID">
                  {profile?.advisorId || "-"}
                </ProfileField>
                <ProfileField icon={Mail} label="Email">
                  <span className="profile-value-email">{profile?.email || "-"}</span>
                </ProfileField>
              </div>

              <h3 className="profile-section-title mt-5">Work Information</h3>
              <div className="profile-info-grid">
                <ProfileField icon={Building2} label="Office">
                  {profile?.office || "-"}
                </ProfileField>
                <ProfileField icon={Clock} label="Office Hours">
                  {profile?.officeHours || "-"}
                </ProfileField>
                <ProfileField icon={Users} label="Assigned Students">
                  {profile?.assignedStudentsCount || 0}
                </ProfileField>
              </div>
            </div>
          </div>

          {/* Programs Supervised Section */}
          <div className="col-12 col-lg-4">
            <div className="profile-card">
              <h3 className="profile-section-title">Programs Supervised</h3>
              {profile?.programsSupervised && profile.programsSupervised.length > 0 ? (
                <div className="programs-list">
                  {profile.programsSupervised.map((program, index) => (
                    <div key={index} className="program-tag">
                      <GraduationCap size={13} strokeWidth={2.2} />
                      {program}
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-muted">No programs assigned.</p>
              )}
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}