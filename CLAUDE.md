# CLAUDE.md — Bank Operations System

## Context Files
Always read these files before starting any task:
- @PROJECT.md — full architecture, DB schema, tech stack, decisions
- @PROGRESS.md — what is done, what is next
- @CONVENTIONS.md — how code is written in this project
- @DECISIONS.md — architectural decisions with reasoning

## Knowledge Base
Before working on any existing feature, check `.claude/knowledge/` for implementation details.
Available knowledge files (updated as features are completed):
- @.claude/knowledge/backend-setup.md — Program.cs pipeline, GlobalExceptionMiddleware, custom exceptions, Swagger setup
- @.claude/knowledge/ci-cd.md — GitHub Actions workflow, Azure OIDC auth, build/deploy jobs, path filter

## Project Summary
A closed, employee-only web application for managing bank clients, accounts, and credits.
- Backend: ASP.NET Core Web API (.NET 10) in `backend/`
- Frontend: React + TypeScript + Chakra UI in `frontend/`
- Database: Azure SQL with EF Core (Code First, TPT inheritance)
- Auth: ASP.NET Identity + JWT

## Custom Commands
Located in `.claude/commands/`:
- `/new-backend-feature` — steps for creating a new backend feature
- `/new-frontend-feature` — steps for creating a new frontend feature
- `/update-docs` — update PROGRESS.md and DECISIONS.md after a feature
- `/create-migration` — steps for EF Core migrations
- `/save-knowledge` — save implementation details to knowledge base

## Rules
- Always read context files before doing anything
- Always check `.claude/knowledge/` before modifying an existing feature
- Work in small steps — never generate an entire module at once
- After each task: briefly explain what was done and why, then update PROGRESS.md
- Never return entities directly from controllers — always use DTOs
- Never write business logic in controllers
- Always use async/await
- English only for all code, comments, and file names

## Git
- Branch strategy: `feature/xxx` → `develop` → Azure deploy
- Commit format: `feat: description`, `fix: description`, `chore: description`, `docs: description`

## After Every Task
1. Explain briefly what was done and why
2. Update `PROGRESS.md`
3. Run `/save-knowledge` if the feature was complex
4. Suggest the next small step