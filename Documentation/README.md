# Documentation Portal

The documentation portal for the HiveMQtt client is built using [Docusaurus 2](https://docusaurus.io/), a modern static website generator.

The API documentation is generated from the C# source code using docfx.

## DocFX

### Installation

```
$ dotnet tool install docfx
```

### Generate API Metadata

```
dotnet tool run docfx metadata ./docfx.json
```

### Generate API Documentation

```
$ dotnet tool run docfx build ./docfx.json
```

## Docusaurus

### Installation

```
$ yarn
```

### Local Development

```
$ yarn start
```

This command starts a local development server and opens up a browser window. Most changes are reflected live without having to restart the server.

### Build

```
$ yarn build
```

This command generates static content into the `build` directory and can be served using any static contents hosting service.

### Deployment

```yarn deploy```

Using SSH:

```
$ USE_SSH=true yarn deploy
```

Not using SSH:

```
$ GIT_USER=<Your GitHub username> yarn deploy
```

If you are using GitHub pages for hosting, this command is a convenient way to build the website and push to the `gh-pages` branch.
