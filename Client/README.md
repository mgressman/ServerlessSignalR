### Execution

npm run dev

### Authentication

the MSAL library stores authentication information in local storage. Since
this is just a very minimal early stage POC for connecting to a SignaR
service securely using Entra ID, there is no work invested to manage the
local storage. Be aware that you may have to manuall go clear authentication
information from local storage if it becomes incorrect or stale.
