# AGENTS Guidelines for the current repository

## General
Before starting any task, read `../.github/.agents/AGENTS.md` if it exists and follow its instructions.
If and only if `../.github/.agents/AGENTS.md` does not exist, read `../.agents/AGENTS.md` if it exists and follow its instructions.

## Project
- .NET 10
- Follow existing C# style, dependency injection, and nullable annotations.
- Keep credentials out of committed files and logs.

## Verification
- Build: dotnet build InvoiceMonitoringCli.sln
