# DiffLog

DiffLog is a CLI that turns git history into polished release notes using AI. It scans commits, groups changes into clear sections, and formats the result for different audiences so you can ship notes faster and keep stakeholders aligned.

## Why it is useful

- Saves time: generates release notes directly from commits.
- Adapts to the audience: developer, end user, social, or executive summaries.
- Consistent output: repeatable structure across releases.
- Flexible delivery: multiple formats, optional files, and CI friendly.

## Features

- Generate notes from tags, ranges, or date filters.
- Multiple audiences and output formats (Markdown, HTML, Plain Text, JSON).
- Optional links to issues/PRs and contributor lists.
- Interactive mode for guided runs.
- Configurable system prompts per audience or per run.

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
- `-o|--output <FILE>`: Output file path (defaults to a generated file in the repository).
- `--print`: Also print the generated release notes to the console.
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
difflog generate --print
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

## Development

Build and pack locally:

```bash
dotnet build src/DiffLog.csproj -c Release
dotnet pack src/DiffLog.csproj -c Release -o ./artifacts
```

## Dependencies

- .NET 10 SDK
- Spectre.Console / Spectre.Console.Cli
- Microsoft.Extensions.AI + OpenAI client
