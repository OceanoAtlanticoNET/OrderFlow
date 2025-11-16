import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useUsers } from "@/hooks/useUsers";
import { useRoles } from "@/hooks/useRoles";
import { ApiError } from "@/services/api/client";
import type { UserDetailResponse, UpdateUserRequest } from "@/types/user";
import type { RoleResponse } from "@/types/role";

export function UserDetail() {
  const { userId } = useParams<{ userId: string }>();
  const navigate = useNavigate();
  const { getUserById, updateUser, lockUser, unlockUser, assignUserRole, removeUserRole } = useUsers();
  const { getRoles } = useRoles();

  const [user, setUser] = useState<UserDetailResponse | null>(null);
  const [allRoles, setAllRoles] = useState<RoleResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);

  // Dialog states
  const [isAddRoleDialogOpen, setIsAddRoleDialogOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<string>("");

  // Form state
  const [formData, setFormData] = useState({
    email: "",
    userName: "",
    phoneNumber: "",
    emailConfirmed: false,
    lockoutEnabled: false,
  });

  useEffect(() => {
    if (userId) {
      loadUser();
      loadRoles();
    }
  }, [userId]);

  const loadUser = async () => {
    if (!userId) return;
    setIsLoading(true);
    const userData = await getUserById(userId);
    if (userData) {
      setUser(userData);
      setFormData({
        email: userData.email,
        userName: userData.userName,
        phoneNumber: userData.phoneNumber || "",
        emailConfirmed: userData.emailConfirmed,
        lockoutEnabled: userData.lockoutEnabled,
      });
    }
    setIsLoading(false);
  };

  const loadRoles = async () => {
    const rolesData = await getRoles();
    if (rolesData) {
      setAllRoles(rolesData);
    }
  };

  const handleUpdateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!userId) return;
    setErrors([]);
    setIsSaving(true);

    try {
      const request: UpdateUserRequest = {
        email: formData.email,
        userName: formData.userName,
        phoneNumber: formData.phoneNumber || undefined,
        emailConfirmed: formData.emailConfirmed,
        lockoutEnabled: formData.lockoutEnabled,
      };
      await updateUser(userId, request);
      loadUser();
    } catch (error) {
      if (error instanceof ApiError) {
        setErrors(error.errors);
      }
    } finally {
      setIsSaving(false);
    }
  };

  const handleLockToggle = async () => {
    if (!user || !userId) return;
    setErrors([]);
    try {
      const isLocked = user.lockoutEnd && new Date(user.lockoutEnd) > new Date();
      if (isLocked) {
        await unlockUser(userId);
      } else {
        await lockUser(userId);
      }
      await loadUser();
    } catch (error) {
      console.error("Failed to toggle lock status", error);
      if (error instanceof ApiError) {
        setErrors(error.errors);
      } else {
        setErrors(["Failed to toggle lock status"]);
      }
    }
  };

  const handleAddRole = async () => {
    if (!userId || !selectedRole) return;
    setErrors([]);
    try {
      await assignUserRole(userId, selectedRole);
      setIsAddRoleDialogOpen(false);
      setSelectedRole("");
      loadUser();
    } catch (error) {
      if (error instanceof ApiError) {
        setErrors(error.errors);
      }
    }
  };

  const handleRemoveRole = async (roleName: string) => {
    if (!userId) return;
    setErrors([]);
    try {
      await removeUserRole(userId, roleName);
      await loadUser();
    } catch (error) {
      console.error("Failed to remove role", error);
      if (error instanceof ApiError) {
        setErrors(error.errors);
      } else {
        setErrors(["Failed to remove role"]);
      }
    }
  };

  const availableRoles = allRoles.filter(
    (role) => !user?.roles.includes(role.roleName)
  );

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-lg">Loading...</div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-lg text-destructive">User not found</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background p-8">
      <div className="max-w-4xl mx-auto space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold">Edit User</h1>
            <p className="text-muted-foreground">{user.email}</p>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate("/admin/users")}>
              Back to Users
            </Button>
            <Button
              variant={(user.lockoutEnd && new Date(user.lockoutEnd) > new Date()) ? "outline" : "destructive"}
              onClick={handleLockToggle}
            >
              {(user.lockoutEnd && new Date(user.lockoutEnd) > new Date()) ? "Unlock User" : "Lock User"}
            </Button>
          </div>
        </div>

        {/* Global Error Display */}
        {errors.length > 0 && (
          <div className="p-4 rounded-md bg-destructive/10 border border-destructive/20">
            <ul className="text-sm text-destructive space-y-1">
              {errors.map((error, index) => (
                <li key={index}>• {error}</li>
              ))}
            </ul>
          </div>
        )}

        {/* User Information Card */}
        <Card>
          <CardHeader>
            <CardTitle>User Information</CardTitle>
            <CardDescription>Update user details</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleUpdateUser} className="space-y-6">
              {errors.length > 0 && (
                <div className="p-3 rounded-md bg-destructive/10 border border-destructive/20">
                  <ul className="text-sm text-destructive space-y-1">
                    {errors.map((error, index) => (
                      <li key={index}>• {error}</li>
                    ))}
                  </ul>
                </div>
              )}

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="email">Email</Label>
                  <Input
                    id="email"
                    type="email"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    disabled={isSaving}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="userName">Username</Label>
                  <Input
                    id="userName"
                    value={formData.userName}
                    onChange={(e) => setFormData({ ...formData, userName: e.target.value })}
                    disabled={isSaving}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="phoneNumber">Phone Number</Label>
                  <Input
                    id="phoneNumber"
                    type="tel"
                    value={formData.phoneNumber}
                    onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                    disabled={isSaving}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="userId">User ID</Label>
                  <Input id="userId" value={user.userId} disabled className="font-mono text-sm" />
                </div>

                <div className="space-y-2">
                  <Label className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      checked={formData.emailConfirmed}
                      onChange={(e) => setFormData({ ...formData, emailConfirmed: e.target.checked })}
                      disabled={isSaving}
                      className="rounded"
                    />
                    Email Confirmed
                  </Label>
                </div>

                <div className="space-y-2">
                  <Label>Account Status</Label>
                  <div>
                    {user.lockoutEnd && new Date(user.lockoutEnd) > new Date() ? (
                      <Badge variant="destructive">Locked</Badge>
                    ) : (
                      <Badge variant="outline">Active</Badge>
                    )}
                  </div>
                </div>
              </div>

              <div className="flex justify-end">
                <Button type="submit" disabled={isSaving}>
                  {isSaving ? "Saving..." : "Save Changes"}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>

        {/* Roles Management Card */}
        <Card>
          <CardHeader>
            <div className="flex justify-between items-center">
              <div>
                <CardTitle>Roles</CardTitle>
                <CardDescription>Manage user role assignments</CardDescription>
              </div>
              <Button
                size="sm"
                onClick={() => setIsAddRoleDialogOpen(true)}
                disabled={availableRoles.length === 0}
              >
                Assign Role
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            {user.roles.length === 0 ? (
              <p className="text-sm text-muted-foreground">No roles assigned</p>
            ) : (
              <div className="space-y-2">
                {user.roles.map((role) => (
                  <div
                    key={role}
                    className="flex justify-between items-center p-3 border rounded-lg"
                  >
                    <div className="flex items-center gap-2">
                      <Badge variant="secondary">{role}</Badge>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleRemoveRole(role)}
                    >
                      Remove
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Additional Information Card */}
        <Card>
          <CardHeader>
            <CardTitle>Additional Information</CardTitle>
            <CardDescription>Read-only user details</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Email Confirmed</p>
                <p className="text-lg">{user.emailConfirmed ? "Yes" : "No"}</p>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Two-Factor Enabled</p>
                <p className="text-lg">{user.twoFactorEnabled ? "Yes" : "No"}</p>
              </div>

              {user.lockoutEnd && (
                <div className="space-y-2">
                  <p className="text-sm font-medium text-muted-foreground">Lockout End</p>
                  <p className="text-lg">{new Date(user.lockoutEnd).toLocaleString()}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Add Role Dialog */}
      <Dialog open={isAddRoleDialogOpen} onOpenChange={setIsAddRoleDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Assign Role</DialogTitle>
            <DialogDescription>Assign a new role to this user</DialogDescription>
          </DialogHeader>

          {errors.length > 0 && (
            <div className="p-3 rounded-md bg-destructive/10 border border-destructive/20">
              <ul className="text-sm text-destructive space-y-1">
                {errors.map((error, index) => (
                  <li key={index}>• {error}</li>
                ))}
              </ul>
            </div>
          )}

          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="role">Select Role</Label>
              <Select value={selectedRole} onValueChange={setSelectedRole}>
                <SelectTrigger>
                  <SelectValue placeholder="Choose a role" />
                </SelectTrigger>
                <SelectContent>
                  {availableRoles.map((role) => (
                    <SelectItem key={role.roleId} value={role.roleName}>
                      {role.roleName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setIsAddRoleDialogOpen(false);
                setSelectedRole("");
              }}
            >
              Cancel
            </Button>
            <Button onClick={handleAddRole} disabled={!selectedRole}>
              Assign Role
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
