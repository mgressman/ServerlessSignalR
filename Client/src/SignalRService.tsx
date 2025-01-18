import * as signalR from "@microsoft/signalr";
import { HUB_BASE_URL } from "./constants";

export const connectToSignalR = async (
  accessToken: string
): Promise<signalR.HubConnection | undefined> => {
  // Replace with your Azure Function URL
  const hubUrl: string = `${HUB_BASE_URL}/api`;

  try {
    // Create the connection to SignalR using token authentication
    const connection: signalR.HubConnection = new signalR.HubConnectionBuilder()
      .configureLogging(signalR.LogLevel.Debug)
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken, // Use the Azure AD token for authentication
      })
      .build();

    // Setup event handlers
    connection.on("newConnection", (message: string) => {
      console.log("New connection with message:", message);
    });

    connection.on("newMessage", (message: string) => {
      console.log("Message from server:", message);
    });

    connection.onclose((error: Error | undefined) => {
      console.error("SignalR connection closed", error);
    });

    // Start the connection
    await connection.start();
    console.log("SignalR connection state:", connection.state);
    console.log("Connected to SignalR");

    return connection;
  } catch (error) {
    console.error("Error connecting to SignalR:", error);
    return undefined; // Return undefined if connection fails
  }
};
