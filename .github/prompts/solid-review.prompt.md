---
description: "Review code changes for SOLID principle violations with concrete findings and remediation"
name: "SOLID Review"
argument-hint: "Scope to review (PR diff, file paths, class/module, or feature area)"
agent: "agent"
---
Perform a SOLID-focused review for: ${input:Scope to review}

Review objective:
- Determine whether the changes violate SOLID principles.
- Prioritize high-confidence design findings over style comments.
- Include likely design smells when they have practical impact.
- Explain impact on maintainability, extensibility, and testability.

Review process:
1. Determine scope from input. If input is empty, review the whole module/folder in context.
2. Identify changed/affected classes, interfaces, and dependency boundaries.
3. Evaluate each relevant area against:
- Single Responsibility Principle (SRP)
- Open/Closed Principle (OCP)
- Liskov Substitution Principle (LSP)
- Interface Segregation Principle (ISP)
- Dependency Inversion Principle (DIP)
4. For every finding, validate with concrete code references and explain why it is a violation.
5. Suggest minimal, practical refactoring steps aligned with current project conventions.

Output format:
1. Findings by SOLID Principle
- Principle: SRP | OCP | LSP | ISP | DIP
- Severity: Critical | High | Medium | Low
- File reference with line
- Violation explanation
- Suggested refactor

2. Compliant Areas
- Briefly list places that follow SOLID well.

3. Open Questions / Assumptions
- Note uncertainty that may change conclusions.

4. Residual Risks
- Note design debt not in changed lines but likely impacted.

Rules:
- If no SOLID issues are found, state explicitly: "No SOLID violations found in reviewed scope."
- Distinguish confirmed violations from likely design smells.
- Keep summary brief; findings are primary.
- Align recommendations with [copilot instructions](../copilot-instructions.md).
