import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useAuth } from "@/hooks/useAuth";

export function DashboardPage() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-8">
      <div className="max-w-4xl mx-auto space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold">OrderFlow Dashboard</h1>
          <Button variant="outline" onClick={handleLogout}>
            Logout
          </Button>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Welcome back!</CardTitle>
            <CardDescription>
              You are successfully authenticated with the OrderFlow Identity API
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm font-medium text-muted-foreground">
                User Email
              </p>
              <p className="text-lg">{user?.email || "N/A"}</p>
            </div>

            <div>
              <p className="text-sm font-medium text-muted-foreground">
                User ID
              </p>
              <p className="text-lg font-mono text-sm">
                {user?.userId || "N/A"}
              </p>
            </div>

            <div>
              <p className="text-sm font-medium text-muted-foreground">Role</p>
              <p className="text-lg">{user?.roles?.[0] || "N/A"}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>🎓 Learning Resources</CardTitle>
            <CardDescription>
              This project demonstrates modern full-stack architecture
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <ul className="space-y-2 text-sm">
              <li>✅ .NET Aspire Orchestration with Service Discovery</li>
              <li>✅ JWT Bearer Authentication with ASP.NET Core Identity</li>
              <li>✅ React 19 with TypeScript and Vite</li>
              <li>✅ shadcn/ui Component Library with Tailwind CSS</li>
              <li>✅ Feature-based Architecture Pattern</li>
              <li>✅ PostgreSQL with Auto-Migrations</li>
            </ul>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
