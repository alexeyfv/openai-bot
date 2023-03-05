# Open AI Telegram Bot

A simple service for connecting `Telegram Bot` to the `Open AI API`.

## Features

1. Temporary in-memory message history. User can view it and clear it.

2. Flexible rules stored in SQLite database for managing access:

    2.1 General rule for all users (a quota of 50 requests or 65 536 tokens).

    2.2 User-specific rules.

    2.3 Unlimited access.

3. Supported commands

    `/message` - Send a message to bot.

    `/jailbreak` - Inject jailbreak prompt to the message (Not implemented yet).

    `/clear` - Clear message history.

    `/history` - Get message history.

    `/remaining` - Check remaining resources.

    `/unlimited` - Get unlimited access.

    `/id` - Get your Telegram Id.

## Required tools

- .NET 7
- Docker (if you want to run in Docker container)

## Usage

### Local

1. Specify environment variables in `launchSettings.json`

    ``` json
    "environmentVariables": {
      "DOTNET_ENVIRONMENT": "Development",
      "TELEGRAM_TOKEN": "",
      "OPENAI_API_KEY": "",
      "DBPATH": "./database/local.sqlite",
      "BOT_NAME": ""
    }
    ```

2. Execute `dotnet run`

### Docker

1. Specify environment variables in `docker-compose-yaml`

    ``` yaml
    # other content here
    
    services:
      openai-bot:
        container_name: openai-bot
        image: openai-bot:latest
        environment:
          - TELEGRAM_TOKEN=TOKEN
          - OPENAI_API_KEY=KEY
          - DBPATH=/app/data/local.sqlite
          - BOT_NAME=NAME
        volumes:
          - ./database/:/app/data
          
    # other content here
    ```

2. Execute the following commands

    ``` bash
    docker build . -t openai-bot
    docker compose up
    ```
