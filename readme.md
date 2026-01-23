<p align="center">
  <img src="assets/logo.svg" alt="DiffLog Logo" width="300" />
</p>

> *llm crafted release notes from your git history*

# DiffLog

DiffLog is a CLI tool that turns git history into polished release notes using AI. It scans commits, groups changes into clear sections, and formats the result for different audiences so you can ship release notes and changelogs faster.

## Features

- Generate notes from tags, ranges, or date filters.
- Multiple audiences and output formats (Markdown, HTML, Plain Text, JSON).
- Optional links to issues/PRs and contributor lists.
- Interactive mode for guided runs.
- Configurable system prompts per audience or per run.
- CI friendly command-line interface.

## Installation

Install as a .NET tool:

```bash
dotnet tool install -g DiffLog
```

## Quick start

```bash
difflog generate -i
```

## Commands

### generate

Generate release notes from git history.

```bash
difflog generate [options]
```

Options:

- `-p|--path <PATH>`: Repository path (default `.`).
- `-f|--from <REF>`: Start reference (tag/branch/commit).
- `-t|--to <REF>`: End reference (default `HEAD`).
- `-a|--audience <AUDIENCE>`: Developers, EndUsers, SocialMedia, Executive.
- `--format <FORMAT>`: Markdown, Html, PlainText, Json.
- `-o|--output <FILE>`: Output file path.
- `--from-date <DATE>`: Include commits from date (yyyy-MM-dd).
- `--to-date <DATE>`: Include commits until date (yyyy-MM-dd).
- `--no-links`: Exclude issue/PR links.
- `--no-contributors`: Exclude contributor list.
- `--repo-url <URL>`: Repository URL override.
- `--exclude <CATEGORIES>`: Comma-separated categories to exclude.
- `--system-prompt <PROMPT>`: Override the system prompt for this run.
- `--system-prompt-file <FILE>`: Read system prompt from file.
- `-i|--interactive`: Guided interactive mode.

Examples:

```bash
difflog generate --from v1.0.0 --to v1.1.0 --format Markdown
difflog generate -a EndUsers --format Html --output release-notes.html
difflog generate --system-prompt-file prompts/social.txt
```

### config

Store OpenAI settings and optional audience-specific system prompts.

```bash
difflog config [options]
```

Options:

- `--api-key <KEY>`: OpenAI API key.
- `--base-url <URL>`: Base URL for OpenAI-compatible APIs.
- `--model <MODEL>`: Model name (default `gpt-4o`).
- `--audience <AUDIENCE>`: Audience to set a custom system prompt for.
- `--system-prompt <PROMPT>`: Prompt to store for that audience.
- `--system-prompt-file <FILE>`: Read the prompt from a file.
- `-i|--interactive`: Prompt for missing values.

Examples:

```bash
difflog config --api-key sk-... --model gpt-4o
difflog config --audience Developers --system-prompt-file prompts/dev.txt
```

### tags

List tags in a repository.

```bash
difflog tags --path .
```

## Configuration

DiffLog reads configuration from environment variables or the saved config file:

- `OPENAI_API_KEY`
- `OPENAI_BASE_URL`
- `OPENAI_MODEL`

The `difflog config` command saves values to a config file (location is printed after saving). Environment variables always take precedence.

System prompt resolution order:

1. `--system-prompt` / `--system-prompt-file`
2. Stored prompt for the selected audience
3. Built-in defaults

## GitHub Actions

Use DiffLog in your CI/CD pipeline to automatically generate release notes. Add your OpenAI API key as a repository secret (`OPENAI_API_KEY`).

```yaml
name: Generate Release Notes

on:
  push:
    tags:
      - 'v*'

jobs:
  release-notes:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Required for full git history

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Install DiffLog
        run: dotnet tool install -g DiffLog

      - name: Generate Release Notes
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          # Optional: override model or base URL
          # OPENAI_MODEL: gpt-4o
          # OPENAI_BASE_URL: https://your-endpoint.openai.azure.com
        run: |
          # Get the previous tag
          PREV_TAG=$(git describe --tags --abbrev=0 HEAD^ 2>/dev/null || echo "")
          CURRENT_TAG=${GITHUB_REF#refs/tags/}
          
          if [ -n "$PREV_TAG" ]; then
            difflog generate --from $PREV_TAG --to $CURRENT_TAG --format Markdown -o release-notes.md
          else
            difflog generate --to $CURRENT_TAG --format Markdown -o release-notes.md
          fi

      - name: Create GitHub Release
        uses: softprops/action-gh-release@v2
        with:
          body_path: release-notes.md
```

For scheduled changelog generation or PR-based workflows:

```yaml
- name: Generate Weekly Changelog
  env:
    OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
  run: |
    difflog generate --from-date $(date -d '7 days ago' +%Y-%m-%d) --format Markdown -o changelog.md
```

## Development

Build and pack locally:

```bash
dotnet build src/DiffLog.csproj -c Release
dotnet pack src/DiffLog.csproj -c Release -o ./artifacts
```

or use `test-local-tool.ps1` script to pack and install the tool from source.


## Dependencies

- .NET 10 SDK
- Spectre.Console / Spectre.Console.Cli
- Microsoft.Extensions.AI + OpenAI client
