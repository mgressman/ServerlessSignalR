import { Configuration, LogLevel } from "@azure/msal-browser";
import { CLIENT_ID, TOKEN_AUTHORITY } from "./constants";

export const msalConfig: Configuration = {
  auth: {
    clientId: CLIENT_ID, // Azure AD App Client ID
    authority: TOKEN_AUTHORITY, // Azure AD Tenant ID
    redirectUri: window.location.origin, // The redirect URI for your app
  },
  cache: {
    cacheLocation: "localStorage", // Configures where to store the auth tokens
    storeAuthStateInCookie: true, // Set this to true for IE11/Edge support
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Info, // Sets the log level
      loggerCallback: (
        _level: LogLevel,
        message: string,
        containsPii: boolean
      ) => {
        if (!containsPii) {
          console.log(message); // Logs messages that do not contain PII
        }
      },
    },
  },
};
