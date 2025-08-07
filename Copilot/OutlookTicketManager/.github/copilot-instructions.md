# Copilot Instructions - Outlook Ticket Manager

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is a Blazor Server application for managing tickets based on Outlook 365 emails integration.

## Technologies Stack
- .NET 7 with ASP.NET Core
- Blazor Server
- Entity Framework Core with SQLite
- Microsoft Graph SDK for Outlook integration
- SignalR for real-time updates
- Bootstrap CSS framework

## Architecture Guidelines
- Follow Clean Architecture principles
- Use Repository pattern for data access
- Implement proper separation of concerns
- Use dependency injection throughout the application
- Follow async/await patterns for Graph API calls

## Security Guidelines
- Use OAuth2 with Azure AD for authentication
- Never store credentials in plain text
- Use secure configuration methods (appsettings.json, user secrets, environment variables)
- Implement proper error handling for Graph API calls

## Code Style
- Use C# naming conventions
- Implement proper logging with ILogger
- Use meaningful variable and method names
- Add XML documentation for public methods
- Follow SOLID principles
