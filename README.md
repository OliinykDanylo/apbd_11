# apbd_11
To run this application, you must provide an appsettings.json configuration file in the root of your project. The file should include the following sections:
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "DefaultConnection": "<YOUR_DATABASE_CONNECTION_STRING>"
  },

  "Jwt": {
    "Key": "<YOUR_SECRET_JWT_KEY>",
    "Issuer": "DeviceHubUpdApp",
    "Audience": "DeviceHubUpdUsers"
  }
}
