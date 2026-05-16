import { useEffect, useMemo, useRef, useState } from "react";
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

          <Button
            label="Create User"
            icon="pi pi-user-plus"
            onClick={handleCreateUser}
            loading={submitting}
            disabled={submitting || linksLoading || availableLinks.length === 0}
          />
        </div>
      </PageSectionCard>

      <PageSectionCard
        title="Managed Users"
        subtitle="View all locally managed student and advisor accounts"
      >
        {users.length === 0 ? (
          <div className="admin-empty-state">No managed users found.</div>
        ) : (
          <div className="row g-3">
            {users.map((user) => (
              <div key={user.userId} className="col-12 col-xl-6">
                <div className="admin-user-card">
                  <div className="admin-user-card__top">
                    <div>
                      <div className="admin-user-card__username">{user.username}</div>
                      <div className="admin-user-card__linked-name">{user.linkedDisplayName}</div>
                    </div>

                    <div className="d-flex gap-2 flex-wrap justify-content-end">
                      <Tag value={user.role.toUpperCase()} severity="info" />
                      <Tag
                        value={user.isActive ? "ACTIVE" : "INACTIVE"}
                        severity={user.isActive ? "success" : "danger"}
                      />
                    </div>
                  </div>

                  <div className="admin-user-card__meta">
                    <div><strong>Linked Record:</strong> {user.secondaryText}</div>
                    <div><strong>Email:</strong> {user.linkedEmail || "-"}</div>
                    <div><strong>Keycloak ID:</strong> {user.keycloakId || "-"}</div>
                  </div>

                  <div className="admin-user-card__actions">
                    <Button
                      label="Edit"
                      icon="pi pi-pencil"
                      outlined
                      onClick={() => openEditDialog(user)}
                    />
                    <Button
                      label={user.isActive ? "Deactivate" : "Reactivate"}
                      icon={user.isActive ? "pi pi-ban" : "pi pi-refresh"}
                      severity={user.isActive ? "danger" : "success"}
                      outlined
                      loading={actionUserId === user.userId}
                      onClick={() => (user.isActive ? handleDeactivateUser(user) : handleReactivateUser(user))}
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </PageSectionCard>

      <Dialog
        header={editingUser ? `Reset Password for ${editingUser.username}` : "Reset Password"}
        visible={editDialogOpen}
        style={{ width: "32rem", maxWidth: "95vw" }}
        onHide={() => {
          if (savingEdit) return;
          setEditDialogOpen(false);
          setEditingUser(null);
        }}
      >
        <div className="d-flex flex-column gap-3">
          <div>
            <label className="admin-form-label">New Password</label>
            <Password
              value={editPassword}
              onChange={(event) => setEditPassword(event.target.value)}
              feedback={false}
              toggleMask
              className="w-100 admin-password"
              inputClassName="w-100"
              placeholder="Enter the new password"
            />
          </div>

          <div className="text-muted small">
            This action only updates the user's password. Username, role, and linked record stay unchanged.
          </div>

          <div className="d-flex justify-content-end gap-2 mt-2">
            <Button
              label="Cancel"
              text
              onClick={() => {
                setEditDialogOpen(false);
                setEditingUser(null);
              }}
            />
            <Button label="Save Changes" onClick={handleSaveEdit} loading={savingEdit} />
          </div>
        </div>
      </Dialog>
    </div>
  );
}