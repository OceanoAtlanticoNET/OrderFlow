import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { useRoles } from "@/hooks/useRoles";
import { ApiError } from "@/services/api/client";
import type { RoleResponse, CreateRoleRequest, UpdateRoleRequest } from "@/types/role";

export function RoleManagement() {
  const navigate = useNavigate();
  const { getRoles, createRole, updateRole, deleteRole } = useRoles();

  const [roles, setRoles] = useState<RoleResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Dialog states
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<RoleResponse | null>(null);
  const [errors, setErrors] = useState<string[]>([]);

  // Form state
  const [roleName, setRoleName] = useState("");

  useEffect(() => {
    loadRoles();
  }, []);

  const loadRoles = async () => {
    setIsLoading(true);
    const rolesData = await getRoles();
    if (rolesData) {
      setRoles(rolesData);
    }
    setIsLoading(false);
  };

  const handleCreateRole = async () => {
    setErrors([]);
    try {
      const request: CreateRoleRequest = { roleName };
      await createRole(request);
      setIsCreateDialogOpen(false);
      setRoleName("");
      loadRoles();
    } catch (error) {
      if (error instanceof ApiError) {
        setErrors(error.errors);
      }
    }
  };

  const handleUpdateRole = async () => {
    if (!selectedRole) return;
    setErrors([]);
    try {
      const request: UpdateRoleRequest = { roleName };
      await updateRole(selectedRole.roleId, request);
      setIsEditDialogOpen(false);
      setRoleName("");
      setSelectedRole(null);
      loadRoles();
    } catch (error) {
      if (error instanceof ApiError) {
        setErrors(error.errors);
      }
    }
  };

  const handleDeleteRole = async () => {
    if (!selectedRole) return;
    setErrors([]);
    try {
      await deleteRole(selectedRole.roleId);
      setIsDeleteDialogOpen(false);
      setSelectedRole(null);
      loadRoles();
    } catch (error) {
      if (error instanceof ApiError) {
        setErrors(error.errors);
      }
    }
  };

  const openEditDialog = (role: RoleResponse) => {
    setSelectedRole(role);
    setRoleName(role.roleName);
    setIsEditDialogOpen(true);
  };

  const openDeleteDialog = (role: RoleResponse) => {
    setSelectedRole(role);
    setIsDeleteDialogOpen(true);
  };

  const resetForm = () => {
    setRoleName("");
    setErrors([]);
    setSelectedRole(null);
  };

  return (
    <div className="min-h-screen bg-background p-8">
      <div className="max-w-5xl mx-auto space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold">Role Management</h1>
            <p className="text-muted-foreground">Manage system roles</p>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate("/admin/dashboard")}>
              Back to Dashboard
            </Button>
            <Button onClick={() => setIsCreateDialogOpen(true)}>
              Create Role
            </Button>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Roles</CardTitle>
            <CardDescription>A list of all roles in the system</CardDescription>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="text-center py-8">Loading...</div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Role Name</TableHead>
                    <TableHead>Role ID</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {roles.map((role) => (
                    <TableRow key={role.roleId}>
                      <TableCell className="font-medium">{role.roleName}</TableCell>
                      <TableCell className="font-mono text-sm text-muted-foreground">
                        {role.roleId}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => openEditDialog(role)}
                          >
                            Edit
                          </Button>
                          <Button
                            variant="destructive"
                            size="sm"
                            onClick={() => openDeleteDialog(role)}
                          >
                            Delete
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Create Role Dialog */}
      <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create New Role</DialogTitle>
            <DialogDescription>Add a new role to the system</DialogDescription>
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
              <Label htmlFor="create-roleName">Role Name</Label>
              <Input
                id="create-roleName"
                value={roleName}
                onChange={(e) => setRoleName(e.target.value)}
                placeholder="e.g., Manager, Supervisor"
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => { setIsCreateDialogOpen(false); resetForm(); }}>
              Cancel
            </Button>
            <Button onClick={handleCreateRole}>Create</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Edit Role Dialog */}
      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Role</DialogTitle>
            <DialogDescription>Update role information</DialogDescription>
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
              <Label htmlFor="edit-roleName">Role Name</Label>
              <Input
                id="edit-roleName"
                value={roleName}
                onChange={(e) => setRoleName(e.target.value)}
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => { setIsEditDialogOpen(false); resetForm(); }}>
              Cancel
            </Button>
            <Button onClick={handleUpdateRole}>Save Changes</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Role</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete the role "{selectedRole?.roleName}"? This action cannot be undone and may fail if users are assigned to this role.
            </DialogDescription>
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

          <DialogFooter>
            <Button variant="outline" onClick={() => { setIsDeleteDialogOpen(false); resetForm(); }}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDeleteRole}>
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
