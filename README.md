# Lazerboard

A database of all beatmap leaderboards as shown in [osu!lazer](https://osu.ppy.sh/wiki/en/Help_centre/Upgrading_to_lazer) with live updates using the [osu! API](https://osu.ppy.sh/docs/index.html#get-scores). 

It provides an ability to view leaderboard scores with different filters and see various user statistics based on their plays and placements, including global rankings.

## Building the project

In order to build the project, install [Docker](https://www.docker.com/) on your machine.

There are two build configurations:

- Dev (`compose.dev.yaml`)
- Production (`compose.yaml`)

If you are looking to contribute, use the **dev** configuration. 
It can be run with the following command:
````
docker-compose -f compose.dev.yaml up
````

The configuration is edited through a .env file (called `dev.env` for the Dev configuration). An example of a .env file is provided below:

````
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=lazerleaderboards
POSTGRES_HOST=localhost
PGDATA=/data/postgres
ASPNETCORE_ENVIRONMENT=Development
Database__Database=lazerleaderboards
Database__Host=db
Database__Port=5432
Database__Username=postgres
Database__Password=postgres
OsuApi__ClientId=00000
OsuApi__ClientSecret=abunchfofbase64
Caching__FileTTL=15
EnableDatabaseSeeding=true
DeleteScoresAfter=1
````

## Testing the project

The backend tests are based on NUnit, Moq and MockQueryable.Moq packages. 
The tests can be located under the `Tests` folder under each backend project.

The frontend is built on React. You can view your changes to the components using Storybook via running the following command in the `Frontend/react` folder:
````
npm run storybook
````

## Contribution guidelines

*Contributions with AI-written code are not tolerated*. If there may be doubts, please disclose whether you've used AI for your Pull Request.

Other than that, *keep your contributions scoped*. For example, if you fixed a specific render bug in the frontend, and then found out that the frontend doesn't handle the requests to the server properly,
do an effort to split these changes into two separate PRs. It would make reviewing the changes a lot easier.