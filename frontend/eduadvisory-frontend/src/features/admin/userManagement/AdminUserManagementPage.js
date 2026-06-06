import { useEffect, useMemo, useRef, useState, useCallback } from "react";
import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";
import { Dropdown } from "primereact/dropdown";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Skeleton } from "primereact/skeleton";
import { Tag } from "primereact/tag";
import { Toast } from "primereact/toast";
import PageSectionCard from "../../../shared/components/PageSectionCard";
import DashboardStatCard from "../../../shared/components/DashboardStatCard";
import { PageHero } from "../../../shared/components/PageHero";
import { adminUserManagementApi } from "../../../services/admin/adminUserManagementApi";
import "./AdminUserManagementPage.css";

const roleOptions = [
  { label: "Student", value: "student" },
  { label: "Advisor", value: "advisor" },
];

export default function AdminUserManagementPage() {
  const toast = useRef(null);
  const [users, setUsers] = useState([]);
  const [availableLinks, setAvailableLinks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [linksLoading, setLinksLoading] = useState(false);

  const [role, setRole] = useState("student");
  const [linkedEntityId, setLinkedEntityId] = useState(null);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [editPassword, setEditPassword] = useState("");
  const [savingEdit, setSavingEdit] = useState(false);
  const [actionUserId, setActionUserId] = useState(null);

  const [userSearch, setUserSearch] = useState("");
  const [userRoleFilter, setUserRoleFilter] = useState("all");
  const [userStatusFilter, setUserStatusFilter] = useState("all");
  const [userPage, setUserPage] = useState(1);
  const USERS_PER_PAGE = 6;

  const filteredUsers = useMemo(() => {
    return users.filter((u) => {
      const matchesSearch =
        !userSearch ||
        u.username.toLowerCase().includes(userSearch.toLowerCase()) ||
        u.linkedDisplayName?.toLowerCase().includes(userSearch.toLowerCase()) ||
        u.linkedEmail?.toLowerCase().includes(userSearch.toLowerCase());
      const matchesRole = userRoleFilter === "all" || u.role === userRoleFilter;
      const matchesStatus =
        userStatusFilter === "all" ||
        (userStatusFilter === "active" ? u.isActive : !u.isActive);
      return matchesSearch && matchesRole && matchesStatus;
    });
  }, [users, userSearch, userRoleFilter, userStatusFilter]);

  const totalUserPages = Math.ceil(filteredUsers.length / USERS_PER_PAGE);
  const paginatedUsers = filteredUsers.slice(
    (userPage - 1) * USERS_PER_PAGE,
    userPage * USERS_PER_PAGE
  );

  const resetUserPage = useCallback(() => setUserPage(1), []);

  const getUserPageNumbers = () => {
    if (totalUserPages <= 5) return Array.from({ length: totalUserPages }, (_, i) => i + 1);
    if (userPage <= 3) return [1, 2, 3, 4, "...", totalUserPages];
    if (userPage >= totalUserPages - 2) return [1, "...", totalUserPages - 3, totalUserPages - 2, totalUserPages - 1, totalUserPages];
    return [1, "...", userPage - 1, userPage, userPage + 1, "...", totalUserPages];
  };

  const getAvatarInitials = (username) =>
    username ? username.slice(0, 2).toUpperCase() : "??";

  const getRoleColor = (role) =>
    role === "advisor" ? "avatar-advisor" : "avatar-student";

  const showSuccess = (detail) => {
    toast.current?.show({
      severity: "success",
      summary: "Success",
      detail,
      life: 3000,
    });
  };

  const showError = (detail) => {
    toast.current?.show({
      severity: "error",
      summary: "Error",
      detail: String(detail),
      life: 4000,
    });
  };

  const stats = useMemo(() => {
    const total = users.length;
    const active = users.filter((user) => user.isActive).length;
    const studentCount = users.filter((user) => user.role === "student").length;
    const advisorCount = users.filter((user) => user.role === "advisor").length;

    return { total, active, studentCount, advisorCount };
  }, [users]);

  const loadUsers = async () => {
    setLoading(true);

    try {
      const response = await adminUserManagementApi.getUsers();
      setUsers(response.data || []);
    } catch (error) {
      console.error(error);
      showError(error?.response?.data ?? error?.message ?? "Failed to load users.");
    } finally {
      setLoading(false);
    }
  };

  const loadAvailableLinks = async (nextRole) => {
    setLinksLoading(true);

    try {
      const response = await adminUserManagementApi.getAvailableLinks(nextRole);
      const links = response.data || [];
      setAvailableLinks(links);
      setLinkedEntityId(links[0]?.linkedEntityId ?? null);
    } catch (error) {
      console.error(error);
      setAvailableLinks([]);
      setLinkedEntityId(null);
      showError(error?.response?.data ?? error?.message ?? "Failed to load available records.");
    } finally {
      setLinksLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  useEffect(() => {
    loadAvailableLinks(role);
  }, [role]);

  const handleCreateUser = async () => {
    if (!linkedEntityId || !username.trim() || !password.trim()) {
      showError("Role, linked record, username, and password are required.");
      return;
    }

    try {
      setSubmitting(true);

      await adminUserManagementApi.createUser({
        role,
        linkedEntityId,
        username: username.trim(),
        password: password.trim(),
      });

      showSuccess("User created successfully.");
      setUsername("");
      setPassword("");
      await Promise.all([loadUsers(), loadAvailableLinks(role)]);
    } catch (error) {
      console.error(error);
      showError(error?.response?.data ?? error?.message ?? "Failed to create user.");
    } finally {
      setSubmitting(false);
    }
  };

  const openEditDialog = (user) => {
    setEditingUser(user);
    setEditPassword("");
    setEditDialogOpen(true);
  };

  const handleSaveEdit = async () => {
    if (!editingUser || !editPassword.trim()) {
      showError("New password is required.");
      return;
    }

    try {
      setSavingEdit(true);

      await adminUserManagementApi.updateUser(editingUser.userId, {
        password: editPassword.trim(),
      });

      showSuccess("Password updated successfully.");
      setEditDialogOpen(false);
      setEditingUser(null);
      setEditPassword("");
      await loadUsers();
    } catch (error) {
      console.error(error);
      showError(error?.response?.data ?? error?.message ?? "Failed to update user.");
    } finally {
      setSavingEdit(false);
    }
  };

  const handleDeactivateUser = async (user) => {
    try {
      setActionUserId(user.userId);

      await adminUserManagementApi.deactivateUser(user.userId);

      showSuccess(`User ${user.username} was deactivated.`);
      await loadUsers();
    } catch (error) {
      console.error(error);
      showError(error?.response?.data ?? error?.message ?? "Failed to deactivate user.");
    } finally {
      setActionUserId(null);
    }
  };

  const handleReactivateUser = async (user) => {
    try {
      setActionUserId(user.userId);

      await adminUserManagementApi.reactivateUser(user.userId);

      showSuccess(`User ${user.username} was reactivated.`);
      await loadUsers();
    } catch (error) {
      console.error(error);
      showError(error?.response?.data ?? error?.message ?? "Failed to reactivate user.");
    } finally {
      setActionUserId(null);
    }
  };

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Toast ref={toast} position="top-right" />
        <Skeleton width="18rem" height="2rem" className="mb-3" />
        <Skeleton height="8rem" className="mb-4" />
        <Skeleton height="14rem" className="mb-4" />
        <Skeleton height="20rem" />
      </div>
    );
  }

  return (
    <div className="admin-users-page container-fluid p-3 p-md-4">
      <Toast ref={toast} position="top-right" />

      <PageHero
        title="User Management"
        badge="Admin Portal"
        subtitle="Create, update passwords, and activate or deactivate student and advisor accounts"
      />

      <div className="row g-4 mb-4">
        <DashboardStatCard title="Total Users" value={stats.total} icon="pi pi-users" />
        <DashboardStatCard title="Active Users" value={stats.active} icon="pi pi-check-circle" />
        <DashboardStatCard title="Students" value={stats.studentCount} icon="pi pi-graduation-cap" />
        <DashboardStatCard title="Advisors" value={stats.advisorCount} icon="pi pi-briefcase" />
      </div>

      <PageSectionCard
        title="Create Managed User"
        subtitle="Select an unregistered student or advisor, then create their login credentials"
        className="mb-4"
      >
        <div className="row g-3">
          <div className="col-12 col-lg-3">
            <label className="admin-form-label">Role</label>
            <Dropdown
              value={role}
              onChange={(event) => setRole(event.value)}
              options={roleOptions}
              optionLabel="label"
              optionValue="value"
              className="w-100"
            />
          </div>

          <div className="col-12 col-lg-5">
            <label className="admin-form-label">Available {role === "student" ? "Students" : "Advisors"}</label>
            <Dropdown
              value={linkedEntityId}
              onChange={(event) => setLinkedEntityId(event.value)}
              options={availableLinks}
              optionLabel="displayName"
              optionValue="linkedEntityId"
              filter
              loading={linksLoading}
              className="w-100"
              placeholder={linksLoading ? "Loading records..." : "Select a record"}
              itemTemplate={(option) => (
                <div className="admin-dropdown-item">
                  <div className="fw-semibold">{option.displayName}</div>
                  <div className="text-muted small">{option.secondaryText}{option.email ? ` • ${option.email}` : ""}</div>
                </div>
              )}
              valueTemplate={(option) =>
                option ? `${option.displayName}${option.secondaryText ? ` • ${option.secondaryText}` : ""}` : "Select a record"
              }
            />
          </div>

          <div className="col-12 col-lg-2">
            <label className="admin-form-label">Username</label>
            <InputText value={username} onChange={(event) => setUsername(event.target.value)} className="w-100" />
          </div>

          <div className="col-12 col-lg-2">
            <label className="admin-form-label">Password</label>
            <Password
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              feedback={false}
              toggleMask
              className="w-100 admin-password"
              inputClassName="w-100"
            />
          </div>
        </div>

        <div className="d-flex justify-content-between align-items-center flex-wrap gap-3 mt-4">
          <div className="text-muted small">
            {availableLinks.length === 0
              ? `No unregistered ${role === "student" ? "students" : "advisors"} are currently available.`
              : `${availableLinks.length} unregistered ${role === "student" ? "student" : "advisor"} record(s) available.`}
          </div>

          <button
            className="btn btn-dark d-flex align-items-center gap-2"
            onClick={handleCreateUser}
            disabled={submitting || linksLoading || availableLinks.length === 0}
          >
            {submitting ? (
              <span className="spinner-border spinner-border-sm" role="status" />
            ) : (
              <i className="pi pi-user-plus" />
            )}
            Create User
          </button>
        </div>
      </PageSectionCard>

      <PageSectionCard
        title="Managed Users"
        subtitle="View all locally managed student and advisor accounts"
      >
        {users.length === 0 ? (
          <div className="admin-empty-state">
            <i className="pi pi-users" style={{ fontSize: "2rem", color: "#9ca3af", marginBottom: "0.75rem" }} />
            <div className="admin-empty-title">No managed users yet</div>
            <div className="admin-empty-sub">Create your first user account using the form above.</div>
          </div>
        ) : (
          <>
            {/* Filter toolbar */}
            <div className="user-filter-bar">
              <div className="user-search-wrap">
                <i className="pi pi-search user-search-icon" />
                <input
                  type="text"
                  className="user-search-input"
                  placeholder="Search by username, name, or email..."
                  value={userSearch}
                  onChange={(e) => { setUserSearch(e.target.value); resetUserPage(); }}
                />
                {userSearch && (
                  <button className="user-search-clear" onClick={() => { setUserSearch(""); resetUserPage(); }}>✕</button>
                )}
              </div>

              <div className="user-filter-chips">
                <div className="user-filter-group">
                  {["all", "student", "advisor"].map((r) => (
                    <button
                      key={r}
                      className={`user-filter-chip ${userRoleFilter === r ? "active" : ""}`}
                      onClick={() => { setUserRoleFilter(r); resetUserPage(); }}
                    >
                      {r === "all" ? "All Roles" : r.charAt(0).toUpperCase() + r.slice(1) + "s"}
                    </button>
                  ))}
                </div>
                <div className="user-filter-group">
                  {["all", "active", "inactive"].map((s) => (
                    <button
                      key={s}
                      className={`user-filter-chip ${userStatusFilter === s ? "active" : ""}`}
                      onClick={() => { setUserStatusFilter(s); resetUserPage(); }}
                    >
                      {s === "all" ? "All Status" : s.charAt(0).toUpperCase() + s.slice(1)}
                    </button>
                  ))}
                </div>
              </div>
            </div>

            {filteredUsers.length === 0 ? (
              <div className="admin-empty-state">No users match your filters.</div>
            ) : (
              <>
                <div className="user-results-info">
                  Showing {(userPage - 1) * USERS_PER_PAGE + 1}–{Math.min(userPage * USERS_PER_PAGE, filteredUsers.length)} of {filteredUsers.length} user{filteredUsers.length !== 1 ? "s" : ""}
                </div>

                <div className="row g-3">
                  {paginatedUsers.map((user) => (
                    <div key={user.userId} className="col-12 col-xl-6">
                      <div className={`admin-user-card ${!user.isActive ? "admin-user-card--inactive" : ""}`}>
                        <div className="admin-user-card__top">
                          <div className="admin-user-card__identity">
                            <div className={`admin-user-avatar ${getRoleColor(user.role)}`}>
                              {getAvatarInitials(user.username)}
                            </div>
                            <div>
                              <div className="admin-user-card__username">{user.username}</div>
                              <div className="admin-user-card__linked-name">{user.linkedDisplayName}</div>
                            </div>
                          </div>
                          <div className="admin-user-card__badges">
                            <span className={`user-role-badge user-role-badge--${user.role}`}>
                              {user.role.charAt(0).toUpperCase() + user.role.slice(1)}
                            </span>
                            <span className={`user-status-badge ${user.isActive ? "user-status-badge--active" : "user-status-badge--inactive"}`}>
                              <span className="user-status-dot" />
                              {user.isActive ? "Active" : "Inactive"}
                            </span>
                          </div>
                        </div>

                        <div className="admin-user-card__meta">
                          <div className="admin-user-meta-row">
                            <i className="pi pi-id-card admin-user-meta-icon" />
                            <span className="admin-user-meta-label">Linked</span>
                            <span className="admin-user-meta-value">{user.secondaryText || "—"}</span>
                          </div>
                          <div className="admin-user-meta-row">
                            <i className="pi pi-envelope admin-user-meta-icon" />
                            <span className="admin-user-meta-label">Email</span>
                            <span className="admin-user-meta-value">{user.linkedEmail || "—"}</span>
                          </div>
                          <div className="admin-user-meta-row">
                            <i className="pi pi-key admin-user-meta-icon" />
                            <span className="admin-user-meta-label">Keycloak ID</span>
                            <span className="admin-user-meta-value admin-user-meta-mono">{user.keycloakId || "—"}</span>
                          </div>
                        </div>

                        <div className="admin-user-card__actions">
                          <button
                            className="btn btn-sm btn-outline-secondary d-flex align-items-center gap-1"
                            onClick={() => openEditDialog(user)}
                          >
                            <i className="pi pi-pencil" style={{ fontSize: "0.75rem" }} />
                            Edit
                          </button>
                          <button
                            className={`btn btn-sm d-flex align-items-center gap-1 ${user.isActive ? "btn-outline-danger" : "btn-outline-success"}`}
                            disabled={actionUserId === user.userId}
                            onClick={() => (user.isActive ? handleDeactivateUser(user) : handleReactivateUser(user))}
                          >
                            {actionUserId === user.userId ? (
                              <span className="spinner-border spinner-border-sm" role="status" />
                            ) : (
                              <i className={`pi ${user.isActive ? "pi-ban" : "pi-refresh"}`} style={{ fontSize: "0.75rem" }} />
                            )}
                            {user.isActive ? "Deactivate" : "Reactivate"}
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                {totalUserPages > 1 && (
                  <div className="doc-pagination" style={{ marginTop: "1rem" }}>
                    <span className="doc-pagination-info">
                      {(userPage - 1) * USERS_PER_PAGE + 1}–{Math.min(userPage * USERS_PER_PAGE, filteredUsers.length)} of {filteredUsers.length}
                    </span>
                    <div className="doc-pagination-controls">
                      <button className="doc-page-btn" onClick={() => setUserPage((p) => p - 1)} disabled={userPage === 1}>
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M15 18l-6-6 6-6" /></svg>
                      </button>
                      {getUserPageNumbers().map((page, idx) =>
                        page === "..." ? (
                          <span key={`e-${idx}`} className="doc-page-ellipsis">…</span>
                        ) : (
                          <button key={page} className={`doc-page-btn ${userPage === page ? "active" : ""}`} onClick={() => setUserPage(page)}>
                            {page}
                          </button>
                        )
                      )}
                      <button className="doc-page-btn" onClick={() => setUserPage((p) => p + 1)} disabled={userPage === totalUserPages}>
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M9 18l6-6-6-6" /></svg>
                      </button>
                    </div>
                  </div>
                )}
              </>
            )}
          </>
        )}
      </PageSectionCard>

      <Dialog
        visible={editDialogOpen}
        style={{ width: "26rem", maxWidth: "95vw" }}
        header={false}
        closable={!savingEdit}
        onHide={() => {
          if (savingEdit) return;
          setEditDialogOpen(false);
          setEditingUser(null);
        }}
        pt={{ content: { style: { padding: 0 } }, header: { style: { display: "none" } } }}
      >
        <div className="reset-pwd-dialog">
          {/* Header */}
          <div className="reset-pwd-dialog__header">
            <div className="reset-pwd-dialog__icon">
              <i className="pi pi-lock" />
            </div>
            <h3 className="reset-pwd-dialog__title">Reset Password</h3>
            <p className="reset-pwd-dialog__subtitle">
              Update login credentials for this account
            </p>
          </div>

          {/* User info strip */}
          {editingUser && (
            <div className="reset-pwd-dialog__user-strip">
              <div className={`admin-user-avatar admin-user-avatar--sm ${getRoleColor(editingUser.role)}`}>
                {getAvatarInitials(editingUser.username)}
              </div>
              <div className="reset-pwd-dialog__user-info">
                <span className="reset-pwd-dialog__user-name">{editingUser.username}</span>
                <span className="reset-pwd-dialog__user-meta">{editingUser.linkedDisplayName}</span>
              </div>
              <span className={`user-role-badge user-role-badge--${editingUser.role}`}>
                {editingUser.role.charAt(0).toUpperCase() + editingUser.role.slice(1)}
              </span>
            </div>
          )}

          {/* Body */}
          <div className="reset-pwd-dialog__body">
            <label className="reset-pwd-dialog__label">New Password</label>
            <Password
              value={editPassword}
              onChange={(event) => setEditPassword(event.target.value)}
              feedback={false}
              toggleMask
              className="w-100 admin-password"
              inputClassName="w-100 reset-pwd-input"
              placeholder="Enter the new password"
              disabled={savingEdit}
            />
            <p className="reset-pwd-dialog__hint">
              Only the password will be updated — username, role, and linked record remain unchanged.
            </p>
          </div>

          {/* Footer */}
          <div className="reset-pwd-dialog__footer">
            <button
              className="btn btn-outline-secondary"
              disabled={savingEdit}
              onClick={() => {
                setEditDialogOpen(false);
                setEditingUser(null);
              }}
            >
              Cancel
            </button>
            <button
              className="btn btn-dark d-flex align-items-center gap-2"
              disabled={savingEdit}
              onClick={handleSaveEdit}
            >
              {savingEdit && <span className="spinner-border spinner-border-sm" role="status" />}
              Save Changes
            </button>
          </div>
        </div>
      </Dialog>
    </div>
  );
}