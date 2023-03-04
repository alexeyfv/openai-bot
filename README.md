# Open AI Telegram Bot

A simple service for connecting `Telegram Bot` to the `Open AI API`.

## Installation

Required tools:

- .NET 7
- Docker (if you want to run in Docker container)

## Usage

### Local

1. Specify environment variables, for example in `launchSettings.json`

    ``` json
    "environmentVariables": {
      "DOTNET_ENVIRONMENT": "Development",
      "TELEGRAM_TOKEN": "YOUR_TOKEN_HERE",
      "OPENAI_API_KEY": "YOUR_TOKEN_HERE"
    }
    ```

2. Execute `dotnet run`

### Docker

``` bash
docker build . -t TAG_NAME
docker run -e TELEGRAM_TOKEN='YOUR_TOKEN_HERE' -e OPENAI_API_KEY='YOUR_TOKEN_HERE' TAG_NAME
```
