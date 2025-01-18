import React, { useEffect, useState } from "react";
import { useAuth } from "./AuthProvider";
import { connectToSignalR } from "./SignalRService";
import { HubConnection } from "@microsoft/signalr";

const App: React.FC = () => {
  const { user, accessToken, login } = useAuth();
  const [connection, setConnection] = useState<HubConnection | null>(null);

  useEffect(() => {
    if (accessToken) {
      // Connect to SignalR once the token is acquired
      connectToSignalR(accessToken).then((conn) => {
        if (conn) {
          setConnection(conn);
        }
      });
    }
  }, [accessToken]);

  return (
    <div>
      <h1>Azure SignalR + Azure AD Authentication</h1>
      {!user ? (
        <button onClick={login}>Login with Azure AD</button>
      ) : (
        <div>
          <p>Welcome, {user.name}</p>
          {connection ? (
            <p>Connected to SignalR!</p>
          ) : (
            <p>Connecting to SignalR...</p>
          )}
        </div>
      )}
    </div>
  );
};

export default App;
