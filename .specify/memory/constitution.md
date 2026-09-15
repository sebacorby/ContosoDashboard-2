# ContosoDashboard Constitution

## Core Principles

### I. Security and Training Boundaries
This project MUST remain a training-only system. No production deployment, external identity provider, or confidential data handling is permitted without a separate governance review and explicit production hardening. Mock authentication, local-only persistence, and sample data are permitted only for educational exercises. This requirement protects the repository from unsupported security claims and keeps the training scenario aligned with its intended purpose.

### II. Access Control and User Isolation
Every page, service method, and data request MUST enforce authorization before exposing project, task, or notification data. The application MUST treat the current authenticated user as the source of truth for visibility, mutation rights, and role-based access decisions. This rule prevents IDOR failures and ensures each user sees only the data they are entitled to access.

### III. Data Integrity and Model Clarity
All domain models and service contracts MUST reflect the real business rules of the dashboard: users, projects, tasks, ownership, membership, status, and notifications. The code MUST reject invalid relationships, keep foreign keys consistent, and maintain clear ownership boundaries. This keeps the business logic understandable and prevents hidden coupling between data and UI behavior.

### IV. Test-First and Regression Safety
New behavior MUST be specified and validated before implementation. For any defect, security change, or workflow change, a failing test or explicit reproduction step MUST exist before code changes. Regression fixes MUST be proven with a repeatable check before merge. This ensures the training app remains stable while teaching disciplined Spec-Driven Development.

### V. Maintainability and Offline-First Architecture
The system MUST favor clear separation of concerns, small services, and dependency injection over hidden assumptions. Local-first development and infrastructure abstractions are required so the application can run offline without cloud dependencies while remaining migration-ready. This preserves a teachable architecture and reduces setup friction for training scenarios.

## Additional Constraints

The project MUST remain aligned with the README training charter:
- Use mock authentication only unless a project-specific exception is approved.
- Prefer local development defaults such as LocalDB and local files over cloud dependencies.
- Keep security demonstrations explicit and educational, not production-grade assumptions.
- Preserve role-based access and service-level checks as default enforcement.
- Record any material departure from these constraints in the relevant spec or plan before implementation.

## Development Workflow

All changes MUST follow the repository's Spec Kit workflow: define the problem in a spec, create a plan when the change is non-trivial, and verify behavior before completion. Pull requests MUST confirm that authorization, data isolation, and regression checks remain intact. Design changes that alter security posture or architecture MUST be reviewed for training suitability before merge.

## Governance

This Constitution supersedes informal practices and local exceptions for project work in this repository. Amendments require a documented rationale, a version bump, and review of impacted principles and workflows before adoption. All changes to governance, authorization, or architecture must be traceable to a spec or amendment record.

The project MUST review compliance on a regular basis, especially after significant feature additions, security changes, or framework upgrades. Any review finding that conflicts with the principles above requires a corrective action plan and, when necessary, a governance amendment.

**Version**: 1.0.0 | **Ratified**: 2026-09-15 | **Last Amended**: 2026-09-15
