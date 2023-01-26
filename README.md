# RumbiBot

A bot to provide needs for the Boop Troop server

# Usage

Clone the repository and make a file called "appsettings.Dev.json" on the root folder, adding the following variables:

```
{
  "Token": "your-bot-token-goes-here",
  "GuildId": 1234,
  "TwitchClientId": "twitch id goes here",
  "TwitchSecretToken": "twitch secret token goes here",
  "TwitchAuth": "twitch auth goes here"
}

```

Change the respective variables with the actual Guild Id and the bot Token.

Then start the project or do `dotnet run` on the console.
