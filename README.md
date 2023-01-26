# RumbiBot

A bot to provide needs for the A Hat in Time speedrunning discord.

This bot was made specifically to target a single guild, but it can be modified to target multiple ones.

## Getting started

Clone the repository, rename the `appsettings_example.json` file to `appsettings.Dev.json` or to any environment your local machine is using, and populate it with the proper information.

Select Rumbi as a start up project and run it.

Alternatively you can use `dotnet run` in the Rumbi project folder.

## Database

This project uses PostgreSQL and it's needed to run some commands.

Once you install everything, make a new database and call it "RumbiDB".

Then on the appsettings file, you'll need to modify the `ConnectionString` field accordingly:

```
 "ConnectionString": "Server=localhost;Port=5432;Database=RumbiDB;User Id=postgresuser;Password=postgrespassword",
```
## Docker

This project has Docker support, you need to have a `appsettings.Dev.json` configuration file on the root of Rumbi with the proper connection string for the PostgreSQL database. 

Then on the solution root folder run: 

```
docker compose up -d
```

and it should automatically run everything properly.

## Contributing

Contributions and feedback are always welcome through pull requests or issues.
