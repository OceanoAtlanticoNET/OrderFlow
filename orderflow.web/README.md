# OrderFlow Web - React Frontend

Modern React + TypeScript frontend for OrderFlow, built with Vite and shadcn/ui.

## 🛠️ Tech Stack

- **React 19** with TypeScript
- **Vite** - Fast build tool with HMR
- **shadcn/ui** - Accessible component library
- **Tailwind CSS v4** - Utility-first styling
- **React Router v7** - Client-side routing
- **.NET Aspire** - Service orchestration & discovery

## 🚀 Running the Project

### Prerequisites
- Node.js 18+
- .NET 9 SDK
- Docker Desktop (for PostgreSQL)

### Start with Aspire
```bash
# From the solution root
dotnet run --project OrderFlow/OrderFlow.csproj
```

This starts:
- PostgreSQL container
- OrderFlow.Identity API
- orderflow.web (React app)
- Aspire Dashboard

### Access Points
- **React App**: Dynamic port (check Aspire Dashboard)
- **Identity API**: Dynamic port (check Aspire Dashboard)
- **Aspire Dashboard**: https://localhost:17242

## 🎯 Key Features

- **JWT Authentication** with ASP.NET Core Identity
- **Service Discovery** via Aspire environment variables
- **Dark Theme** (Cosmic theme from shadcn)
- **Type-Safe API Client** matching C# DTOs
- **Feature-Based Architecture**

## 📂 Project Structure

```
src/
  features/           # Feature modules
    auth/            
      LoginPage.tsx
      RegisterPage.tsx
      api/
        authApi.ts
    dashboard/
  components/         # Shared components
    ui/              # shadcn/ui components
  hooks/             # Custom React hooks
  types/             # TypeScript types
  lib/               # Utilities
```

## 🔑 Authentication Flow

1. User registers/logs in via Identity API
2. API returns JWT token
3. Token stored in localStorage
4. Sent in `Authorization: Bearer <token>` header
5. Protected routes verify authentication

## 📚 Resources

- [React Docs](https://react.dev)
- [shadcn/ui](https://ui.shadcn.com)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)
- [Vite](https://vite.dev)

