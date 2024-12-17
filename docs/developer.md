# Developer Notes

## Some relevant developer notes:

- In case the HealthCheck payload changes (this is we have to call more or fewer endpoints) all we have to do is change the list of endpoints on the `OdsApiSettings` section of the `appsettings.json` file.

- In case you are running this application from a Docker container, but the Admin Api instance is on the host machine, change the `localhost` to `host.docker.internal` on the `appsettings.json` file. So for example the `ApiUrl` setting would look something like this `https://host.docker.internal:<post>` and the AccessTokenUrl setting would look something like this `https://host.docker.internal:<post>/connect/token`.
