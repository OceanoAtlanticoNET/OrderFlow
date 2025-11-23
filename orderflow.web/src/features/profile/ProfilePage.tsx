import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useProfile } from "@/hooks/useProfile";
import type { UserDetailResponse } from "@/types/user";

export function ProfilePage() {
  const navigate = useNavigate();
  const { getMyProfile } = useProfile();
  const [profile, setProfile] = useState<UserDetailResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    setIsLoading(true);
    const profileData = await getMyProfile();
    if (profileData) {
      setProfile(profileData);
    }
    setIsLoading(false);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="text-lg">Loading...</div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="text-lg text-destructive">Failed to load profile</div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6 p-8">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold">My Profile</h1>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate("/dashboard")}>
              Back to Dashboard
            </Button>
            <Button onClick={() => navigate("/profile/edit")}>
              Edit Profile
            </Button>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Personal Information</CardTitle>
            <CardDescription>Your account details and information</CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Email</p>
                <p className="text-lg">{profile.email}</p>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Username</p>
                <p className="text-lg">{profile.userName}</p>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Phone Number</p>
                <p className="text-lg">{profile.phoneNumber || "N/A"}</p>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">User ID</p>
                <p className="text-lg font-mono text-sm">{profile.userId}</p>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Roles</p>
                <div className="flex gap-2 flex-wrap">
                  {profile.roles.map((role) => (
                    <Badge key={role} variant="secondary">
                      {role}
                    </Badge>
                  ))}
                </div>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Account Status</p>
                <div>
                  {profile.lockoutEnd && new Date(profile.lockoutEnd) > new Date() ? (
                    <Badge variant="destructive">Locked</Badge>
                  ) : (
                    <Badge variant="outline">Active</Badge>
                  )}
                </div>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Email Confirmed</p>
                <p className="text-lg">{profile.emailConfirmed ? "Yes" : "No"}</p>
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-muted-foreground">Two-Factor Enabled</p>
                <p className="text-lg">{profile.twoFactorEnabled ? "Yes" : "No"}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Security</CardTitle>
            <CardDescription>Manage your password and security settings</CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={() => navigate("/profile/password")}>
              Change Password
            </Button>
          </CardContent>
        </Card>
    </div>
  );
}
