# OrderFlow Web - React Frontend

This is a React + TypeScript frontend application for the OrderFlow system, designed as an educational project to demonstrate modern full-stack architecture with .NET Aspire.

## 🎓 Educational Concepts

### **1. .NET Aspire Orchestration**
- **Service Discovery**: The React app communicates with backend services through Aspire's service discovery mechanism
- **Environment Configuration**: Service URLs are injected via environment variables (`VITE_IDENTITY_URL`)
- **Orchestrated Startup**: Aspire coordinates the startup order - database → Identity API → Web frontend
- **Health Checks**: Services report their health status to Aspire's dashboard

### **2. JWT Bearer Authentication Flow**
- **Token-Based Auth**: After login, the API returns a JWT token that proves user identity
- **Token Storage**: Tokens are stored in localStorage and sent with each authenticated request
- **Authorization Header**: Format: `Authorization: Bearer <token>`
- **Protected Routes**: React Router guards routes that require authentication
- **Token Lifecycle**: Tokens expire after 60 minutes; users must re-login

### **3. React Architecture Patterns**

#### **Feature-Based Organization**
```
src/
  features/           # Organize by domain/feature
    auth/             # Authentication feature
      LoginPage.tsx
      RegisterPage.tsx
      api/            # Feature-specific API layer
        authApi.ts
    dashboard/
      DashboardPage.tsx
```

#### **Custom Hooks Pattern**
- **useAuth()**: Encapsulates authentication state and operations
- **Separation of Concerns**: UI components don't handle auth logic directly
- **Reusability**: Same hook used across multiple components

#### **Component Composition**
- **ProtectedRoute**: Higher-order component wrapping protected pages
- **Loading States**: Handle async operations gracefully
- **Error Boundaries**: Catch and display API errors

### **4. shadcn/ui Component System**
- **Copy-Paste Components**: Components live in your codebase, not node_modules
- **Built on Radix UI**: Accessible, unstyled primitives
- **Tailwind CSS**: Utility-first styling with consistent design tokens
- **Class Variance Authority (cva)**: Type-safe component variants

### **5. Type Safety with TypeScript**
- **Interface Contracts**: Types match C# DTOs exactly (LoginRequest, RegisterRequest, etc.)
- **Compile-Time Safety**: Catch errors before runtime
- **IntelliSense**: Auto-completion for API responses and component props
- **Path Aliases**: `@/` imports resolve to `src/` directory

### **6. API Client Architecture**

#### **Centralized Error Handling**
```typescript
class ApiError extends Error {
  errors: string[]  // Matches C# ValidationProblem format
}
```

#### **Token Management**
```typescript
tokenStorage.setToken()    // Save after login
tokenStorage.getToken()    // Retrieve for API calls
tokenStorage.removeToken() // Clear on logout
```

#### **Async/Await Pattern**
- Modern async flow with try/catch
- Loading states during requests
- Error state management

### **7. React Router v6 Concepts**
- **BrowserRouter**: Client-side routing without page reloads
- **Route Protection**: Redirect unauthenticated users to login
- **Programmatic Navigation**: `useNavigate()` for post-login redirects
- **Catch-All Route**: `path="*"` redirects to home

### **8. Form Handling Best Practices**
- **Controlled Components**: React state drives form inputs
- **Client-Side Validation**: Required fields, email format
- **Server-Side Validation**: API returns detailed error messages
- **Loading States**: Disable inputs during submission
- **User Feedback**: Error lists, success messages

### **9. State Management Strategy**
- **Local State**: `useState` for form inputs
- **Auth State**: Custom hook with useEffect for initialization
- **No Global State Library**: Simple enough without Redux/Zustand
- **Prop Drilling Avoided**: Context-free architecture with hooks

### **10. Vite Build Tool**
- **Fast HMR**: Hot Module Replacement for instant updates
- **ES Modules**: Native ESM for fast dev server
- **Optimized Builds**: Code splitting, tree shaking
- **Environment Variables**: `import.meta.env.VITE_*`

## 🚀 Running the Project

### Prerequisites
- Node.js 18+ installed
- .NET 9 SDK installed
- Docker Desktop running (for PostgreSQL)

### Development Mode
```bash
# From the OrderFlow solution root
dotnet run --project OrderFlow/OrderFlow.csproj
```

This starts:
1. **PostgreSQL** container on port 5432
2. **OrderFlow.Identity** API on https://localhost:7264
3. **orderflow.web** dev server on https://localhost:5173

### Access Points
- **React App**: https://localhost:5173
- **Identity API**: https://localhost:7264/api/auth
- **API Docs (Scalar)**: https://localhost:7264/scalar
- **Aspire Dashboard**: https://localhost:17242

### Testing Authentication
1. Navigate to https://localhost:5173
2. You'll be redirected to `/login`
3. Click "Sign up" to create an account
4. Fill in the registration form (password requires 8+ chars, uppercase, lowercase, number, special char)
5. After registration, you'll be redirected to login
6. Log in with your credentials
7. You'll see the dashboard with your user details

## 📂 Project Structure Explained

### `/src/types/auth.ts`
Contains TypeScript interfaces that mirror C# DTOs:
- Ensures type safety between frontend and backend
- Single source of truth for API contracts

### `/src/features/auth/api/authApi.ts`
API client for authentication endpoints:
- Handles fetch requests with proper headers
- Manages token storage in localStorage
- Converts HTTP errors to typed ApiError objects

### `/src/hooks/useAuth.ts`
Custom hook for authentication:
- Manages auth state (user, token, loading)
- Provides login, register, logout functions
- Initializes auth state on mount from localStorage

### `/src/components/ui/`
shadcn/ui components:
- **Button**: Multiple variants (default, outline, ghost)
- **Card**: Container with header, content, footer sections
- **Input**: Styled form input with focus states
- **Label**: Accessible form labels

### `/src/features/auth/`
Authentication pages:
- **LoginPage**: Email/password login with error handling
- **RegisterPage**: User registration with validation

### `/src/components/ProtectedRoute.tsx`
Route guard component:
- Checks authentication status
- Shows loading state during auth check
- Redirects to login if not authenticated

## 🔑 Key Learning Points

### **Why JWT Tokens?**
- **Stateless**: Server doesn't need to store session data
- **Scalable**: Works across multiple servers
- **Standard**: Industry-standard authentication method

### **Why Feature-Based Structure?**
- **Scalability**: Easy to add new features without conflicts
- **Maintainability**: Related code stays together
- **Team Collaboration**: Multiple developers can work on different features

### **Why Custom Hooks?**
- **Logic Reuse**: Share stateful logic between components
- **Testability**: Test hooks independently from UI
- **Clean Components**: Keep components focused on rendering

### **Why shadcn/ui?**
- **Ownership**: You own the code, modify as needed
- **No Black Box**: Understand exactly what's happening
- **Lightweight**: Only include components you use

### **Why TypeScript?**
- **Catch Errors Early**: Find bugs at compile time
- **Better Refactoring**: Rename with confidence
- **Documentation**: Types serve as inline docs

## 🎯 Next Steps for Learning

1. **Add More Features**: Create order management pages
2. **Error Boundaries**: Implement React error boundaries
3. **State Management**: Explore when you'd need Redux/Zustand
4. **Testing**: Add unit tests with Vitest and React Testing Library
5. **Performance**: Learn about React.memo, useMemo, useCallback
6. **Accessibility**: Improve keyboard navigation and screen readers
7. **Responsive Design**: Optimize for mobile devices
8. **API Caching**: Implement React Query for data fetching
9. **WebSockets**: Add real-time features with SignalR
10. **Deployment**: Learn about production builds and hosting

## 📚 Additional Resources

- **React Docs**: https://react.dev
- **TypeScript Handbook**: https://www.typescriptlang.org/docs/
- **shadcn/ui**: https://ui.shadcn.com
- **Vite Guide**: https://vite.dev/guide/
- **React Router**: https://reactrouter.com
- **.NET Aspire**: https://learn.microsoft.com/dotnet/aspire/

## 🤝 Contributing

This is an educational project. Feel free to:
- Experiment with the code
- Add new features
- Improve the architecture
- Share what you learned

---

**Remember**: The best way to learn is by doing. Break things, fix them, and understand why!

import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
```
