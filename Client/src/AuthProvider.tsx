// src/AuthProvider.tsx
import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from "react";
import { PublicClientApplication, AccountInfo } from "@azure/msal-browser";
import { msalConfig } from "./msalConfig";
import { SCOPES } from "./constants";

interface AuthContextProps {
  user: AccountInfo | null;
  accessToken: string | null;
  login: () => Promise<void>;
}

const AuthContext = createContext<AuthContextProps | undefined>(undefined);

const msalInstance = new PublicClientApplication(msalConfig);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<AccountInfo | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    const initializeMSAL = async () => {
      try {
        // Initialize the PublicClientApplication
        await msalInstance.initialize();

        const accounts = msalInstance.getAllAccounts();
        if (accounts.length > 0) {
          const account = accounts[0];
          setUser(account);

          const token = await msalInstance.acquireTokenSilent({
            account,
            scopes: SCOPES,
          });
          setAccessToken(token.accessToken);
        }
      } catch (error) {
        console.error("Error during MSAL initialization:", error);
      } finally {
        setIsInitialized(true);
      }
    };

    initializeMSAL();
  }, []);

  const login = async () => {
    try {
      if (!isInitialized) {
        console.error("MSAL is not initialized yet");
        return;
      }

      const response = await msalInstance.loginPopup({
        scopes: SCOPES,
      });
      setUser(response.account);

      const token = await msalInstance.acquireTokenSilent({
        account: response.account,
        scopes: SCOPES,
      });
      setAccessToken("token.accessToken");
    } catch (error) {
      console.error("Login failed:", error);
    }
  };

  return (
    <AuthContext.Provider value={{ user, accessToken, login }}>
      {isInitialized ? children : <p>Loading...</p>}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextProps => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
