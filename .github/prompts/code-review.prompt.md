---
description: "Review code changes for defects, regressions, and missing tests with actionable findings"
name: "Code Review"
argument-hint: "Scope to review (PR diff, file paths, or feature area)"
agent: "agent"
---
Perform a focused code review for: ${input:Scope to review}

Review objective:
- Find real defects, behavioral regressions, security issues, performance risks, and maintainability problems.
- Prioritize findings over summary.
- Use balanced strictness: prioritize bugs/regressions first, then important maintainability concerns.
- Be conservative: only report high-confidence findings.

Use this review process:
1. Identify the exact scope from user input. If the scope is empty, review the current git diff.
2. Inspect relevant files and call paths.
3. Validate assumptions with tests or error checks when feasible.
4. Report findings ordered by severity.

Output format:
1. Findings
- Severity: Critical | High | Medium | Low
- File reference with line
- Why this is a problem
- Suggested fix

2. Open Questions / Assumptions
- List anything uncertain that could change the review conclusion.

3. Residual Risks / Test Gaps
- Mention uncovered paths, edge cases, or missing tests.

Rules:
- If no issues are found, explicitly state that no findings were discovered.
- Keep overview brief; findings are the primary output.
- Prefer precise file references and concrete remediation steps.
- Align with project conventions in [copilot instructions](../copilot-instructions.md).
