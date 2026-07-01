---
name: readme-feature-update
description: 'Update README.md when a new feature is added. Use for syncing setup/run docs, API/UI usage, feature list, project structure, validation steps, and troubleshooting so a clean clone can run and verify the new feature.'
argument-hint: 'Feature summary and changed files or modules'
user-invocable: true
---

# README Feature Update

## When to Use
- A new backend or frontend feature is added.
- Existing behavior changes (endpoint contract, UI flow, configuration, env vars, ports, prerequisites).
- Docs drift is likely after implementation.

## Expected Input
- Feature summary.
- Scope of change (API, UI, both).
- Changed files/modules (or git diff scope).
- New run/test/verification steps, if any.

## Procedure
1. Detect documentation impact from code changes.
2. Identify README sections that must change.
3. Update only impacted sections; keep style and structure consistent.
4. Add a short "What changed" subsection for each notable feature update.
5. Add runnable verification steps for a clean clone.
6. Validate accuracy against code and commands.
7. Run verification commands (build/run/test) before finalizing.
8. Notify the user explicitly that README has been updated.
9. Produce a compact change summary.

## Decision Logic

### 1) Determine Impacted Areas
- Backend-only feature:
  - Update API endpoints, request/response examples, backend run/test instructions.
- Frontend-only feature:
  - Update UI flow, user steps, frontend run/build instructions.
- Full-stack feature:
  - Update both API and UI sections plus end-to-end quick-start path.

### 2) Choose Sections to Update
Check and update only what changed:
- Project Overview
- Features
- Prerequisites
- Installation and Setup
- Quick Start
- API Endpoints
- Project Structure
- Development Tips
- Troubleshooting

### 3) Branch for Contract Changes
- If endpoint added/changed:
  - Add method, route, params/body, response shape, and sample call.
- If validation/error behavior changed:
  - Add expected status codes and example error outcomes.
- If configuration changed:
  - Add env vars, default values, and where to set them.

### 4) Branch for Operational Changes
- If startup/test command changed:
  - Update exact command block(s).
- If new dependency/tooling required:
  - Update prerequisites and verification commands.
- If ports changed:
  - Update all occurrences consistently.

### 5) Verification Before Finalizing
- Ensure all commands are copy-paste runnable.
- Ensure endpoint docs match current model/property names.
- Ensure quick-start still works from clean clone.
- Ensure feature appears in Features and at least one usage flow.

## Completion Criteria
- README reflects new feature behavior accurately.
- No stale endpoint signatures or outdated command steps remain.
- Clean-clone run path includes the new feature validation.
- "What changed" is included for the new feature update.
- Changes are concise and limited to impacted sections.

## Output Format
1. Updated README content in-place.
2. User notification message confirming README update completion.
3. Summary of changed sections.
4. Final verification checklist:
- Start backend
- Start frontend
- Execute feature flow
- Confirm expected output/error cases

## Quality Guardrails
- Prefer explicit commands over vague instructions.
- Keep docs implementation-true; do not invent behavior.
- Preserve existing README tone and structure.
- Avoid unrelated rewrites or formatting churn.
