// React 18+ runs components twice in StrictMode for development to detect unexpected side effects
// or issues with component logic. This is causing the signalr connection request to fire
// twice making debugging confusing. Disabled StrictMode while debugging.

//import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import { AuthProvider } from "./AuthProvider"; // Import your AuthProvider

createRoot(document.getElementById("root")!).render(
  //<StrictMode>
  <AuthProvider>
    <App />
  </AuthProvider>
  //</StrictMode>
);
