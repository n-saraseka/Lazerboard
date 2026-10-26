# [Lazerboard](https://lazerboard.melguy.com/)

A database of all beatmap leaderboards as shown in [osu!lazer](https://osu.ppy.sh/wiki/en/Help_centre/Upgrading_to_lazer) with live updates using the [osu! API](https://osu.ppy.sh/docs/index.html#get-scores). 

It provides an ability to view leaderboard scores with different filters and see various user statistics based on their plays and placements, including global rankings.

## Building the project

In order to build the project, install [Docker](https://www.docker.com/) on your machine.

There are two build configurations:

- Dev (`compose.dev.yaml` and `compose.dev.pgbackrest.yaml`)
- Production (`compose.yaml`)

If you are looking to contribute, use the **dev** configuration. 
It can be run with the following command:
````
docker-compose -f compose.dev.yaml up -d
````

The configuration is edited through a .env file. An example of a .env file with explanations is provided in the `.env.example` file.

If you wish to run the configuration with **pgBackRest** with the purpose of testing backups,
you can run the above command and specify the `compose.dev.pgbackrest.yaml` file instead. You will have to add your own `pgbackrest.conf` configuration file to the project's root folder.
The documentation for that file can be found [here](https://pgbackrest.org/configuration.html).

## Testing the project

The backend tests are based on NUnit, Moq and MockQueryable.Moq packages. 
The tests can be located under the `Tests` folder under each backend project.

The frontend is built on React. You can view your changes to the components using Storybook via running the following command in the `Frontend/react` folder:
````
npm run storybook
````

## Monitoring

This project supports infrastructure monitoring via following data sources: **Node Exporter** 
for system information and **Caddy** metrics.

Data scraping is configured through **Prometheus** and **Grafana**. 

Grafana UI is available under the /grafana/ URL on the project domain, 
and can be configured via web settings or the environment variables.

Prometheus can be configured via the `prometheus.yml` file.


## Contribution guidelines

*Contributions with AI-written code are not tolerated*. If there may be doubts, please disclose whether you've used AI for your Pull Request.

Other than that, *keep your contributions scoped*. For example, if you fixed a specific render bug in the frontend, and then found out that the frontend doesn't handle the requests to the server properly,
do an effort to split these changes into two separate PRs. It would make reviewing the changes a lot easier.
