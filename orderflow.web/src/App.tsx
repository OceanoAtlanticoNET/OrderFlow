import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'

function App() {
  const [count, setCount] = useState(0)
  
  // Service reference will be injected by Aspire
  // Access via environment variables or service discovery
  const identityServiceUrl = import.meta.env.VITE_IDENTITY_URL || 'https://localhost:7264'

  return (
    <>
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>OrderFlow - Order Management System</h1>
      <p className="read-the-docs">
        Powered by .NET Aspire + React + Vite
      </p>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
        <p style={{ fontSize: '0.9em', opacity: 0.7 }}>
          Identity Service: {identityServiceUrl}
        </p>
      </div>
    </>
  )
}

export default App
