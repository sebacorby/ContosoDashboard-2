# Research: Document Upload and Management

**Feature**: `001-document-upload-management`  
**Date**: 2026-09-15

## Decision 1: Preserve the existing single-project Blazor Server architecture

**Decision**: Extend the existing .NET 10 Blazor Server application rather than introducing a separate frontend or API application.

**Rationale**: ContosoDashboard is already a single ASP.NET Core/Blazor Server project with Razor Pages, services, EF Core, cookie authentication, and dependency injection. Keeping that shape minimizes change and satisfies the stakeholder constraint against major rewrites.

**Alternatives considered**: Separate SPA + API, additional microservice, or standalone document service. Rejected because they add deployment and authentication complexity that is unnecessary for an offline training application.

## Decision 2: Use SQLite for training metadata persistence

**Decision**: Continue using EF Core 10 with SQLite in the Linux Dev Container.

**Rationale**: The exercise environment cannot run SQL Server LocalDB inside Linux. SQLite is already configured and validated end-to-end, while preserving the existing EF Core DbContext programming model.

**Alternatives considered**: LocalDB on the Windows host or a SQL Server container. Rejected for the lab because they add environment dependencies without improving the feature design.
## Decision 3: Store uploaded content outside the web root

**Decision**: Store files under a configurable local root such as `ContosoDashboard/AppData/uploads`, never under `wwwroot`.

**Rationale**: Files must not be directly addressable as static content. All preview/download access must pass through authorization checks. Persist only relative storage paths in the database.

**Alternatives considered**: `wwwroot/uploads` and absolute paths. Rejected because static serving can bypass authorization and absolute paths reduce portability.

## Decision 4: Generate storage identities independently of user filenames

**Decision**: Preserve the original filename as metadata, but use generated unique storage names organized as `{userId}/{projectId-or-personal}/{uniqueId}.{ext}`.

**Rationale**: This prevents collisions and path traversal while preserving the user-facing filename. The same relative-name pattern can later map to blob names.

**Alternatives considered**: Saving the original filename directly or using database IDs as filenames. Rejected because user names are unsafe/unreliable and database IDs are not available before the file-save-first sequence.

## Decision 5: Keep upload persistence atomic from the user's perspective

**Decision**: Validate and scan first, save file content, then create database metadata; on any later failure remove partial/stored content and return a retryable error.

**Rationale**: This implements the clarified requirement that failed uploads leave neither orphan records nor partial files.
## Decision 6: Separate storage and malware-scanning concerns behind interfaces

**Decision**: Introduce `IFileStorageService` and `IFileScanService`. The training implementation remains offline and local; the scanning implementation is explicitly training-grade and must support deterministic unsafe-file tests without claiming production malware protection.

**Rationale**: The stakeholder requirements demand both offline training and a future cloud migration path. Separate interfaces keep business logic independent from storage and scanner implementations.

**Alternatives considered**: Embed filesystem and scanning logic directly in `DocumentService`, or add a cloud SDK now. Rejected because both violate maintainability or offline constraints.

## Decision 7: Enforce authorization in the service layer and download/preview endpoint

**Decision**: `DocumentService` is the authorization boundary for metadata operations. Any HTTP endpoint that streams content must re-check access using the current user's claims and project/share relationships before opening the file.

**Rationale**: This matches the constitution's access-control principle and prevents IDOR-style access to files outside `wwwroot`.

**Alternatives considered**: UI-only checks or static-file middleware. Rejected because client navigation and guessed identifiers could bypass them.

## Decision 8: Use test-first service/integration coverage plus manual Blazor acceptance testing

**Decision**: Add an xUnit test project using SQLite-backed test contexts for service behavior and filesystem test doubles/temporary directories. Use manual browser validation for the MVP Blazor upload journey, with component tests added where they materially reduce regression risk.

**Rationale**: This satisfies the constitution's test-first requirement while keeping the training implementation focused. Performance targets are validated with repeatable seeded scenarios plus quickstart timing checks.

**Alternatives considered**: Manual testing only. Rejected because authorization, cleanup, validation, and failure behavior need repeatable regression coverage.

## Research Result

All technical unknowns needed for Phase 1 are resolved. No `NEEDS CLARIFICATION` items remain; planning can proceed with the decisions above.