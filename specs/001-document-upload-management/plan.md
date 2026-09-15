# Implementation Plan: Document Upload and Management

**Branch**: `001-document-upload-management` | **Date**: 2026-09-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-document-upload-management/spec.md`

## Summary

Extend the existing ContosoDashboard Blazor Server application with secure offline document upload, browsing, download/preview, sharing, audit, and integration capabilities. Metadata remains in the existing EF Core data layer; uploaded bytes are stored outside `wwwroot` through `IFileStorageService`. Authorization remains service-layer enforced, file delivery revalidates access, and storage/scanning implementations are dependency-injected so the training build stays local while remaining migration-ready.

## Technical Context

**Language/Version**: C# / .NET 10 (`10.0.401` in the Dev Container)  
**Primary Dependencies**: ASP.NET Core Blazor Server, Razor Pages, EF Core 10, EF Core SQLite, cookie authentication, dependency injection  
**Storage**: SQLite for metadata; local filesystem under configurable `AppData/uploads` for file content  
**Testing**: xUnit service/integration tests with SQLite-backed test contexts and temporary filesystem storage; manual Blazor acceptance testing from `quickstart.md`  
**Target Platform**: Existing ASP.NET Core web application running in the Linux Dev Container and accessible from a desktop browser  
**Project Type**: Single-project web application plus a test project  
**Performance Goals**: 25 MB upload within 30 seconds; 500-document list within 2 seconds; search within 2 seconds; preview within 3 seconds  
**Constraints**: Offline-capable training environment; no cloud runtime dependency; file content outside `wwwroot`; 25 MB/file; supported-type whitelist; service-level authorization; permanent delete only; training-grade malware scan explicitly not production protection  
**Scale/Scope**: Initial release for the existing ContosoDashboard user base; list/search behavior validated at 500 documents; MVP prioritizes User Story 1 upload and My Documents browsing

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Security and Training Boundaries — PASS**: local-only storage, mock authentication, and training scanner behavior are explicit; no production-security claims or cloud runtime dependencies are introduced.
- **Access Control and User Isolation — PASS**: metadata operations and file-content endpoints enforce current-user authorization using role, project membership, ownership, and explicit shares.
- **Data Integrity and Model Clarity — PASS**: integer document identifiers, text categories, explicit relationships, safe storage paths, and atomic cleanup rules are defined.
- **Test-First and Regression Safety — PASS**: service/storage/authorization behavior receives automated tests before implementation closure; quickstart provides repeatable manual acceptance validation.
- **Maintainability and Offline-First Architecture — PASS**: `IFileStorageService` and `IFileScanService` isolate infrastructure choices and preserve future migration options.

No constitution violations require exceptions.
## Project Structure

### Documentation (this feature)

```text
specs/001-document-upload-management/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── document-access.md
│   └── storage-service.md
└── tasks.md
```

### Source Code (repository root)

```text
ContosoDashboard/
├── Controllers/
│   └── DocumentsController.cs          # authorized download/preview delivery
├── Data/
│   └── ApplicationDbContext.cs         # new document DbSets/configuration
├── Models/
│   ├── Document.cs
│   ├── DocumentTag.cs
│   ├── DocumentShare.cs
│   ├── DocumentTask.cs
│   └── DocumentActivity.cs
├── Pages/
│   ├── Documents.razor                 # My Documents + upload/browse MVP
│   ├── ProjectDetails.razor            # project document integration
│   ├── Tasks.razor                     # task document integration when task detail exists
│   └── Index.razor                     # recent-documents/count integration
├── Services/
│   ├── IDocumentService.cs
│   ├── DocumentService.cs
│   ├── IFileStorageService.cs
│   ├── LocalFileStorageService.cs
│   ├── IFileScanService.cs
│   └── TrainingFileScanService.cs
├── Shared/
│   └── NavMenu.razor                    # Documents navigation entry
├── AppData/uploads/                     # runtime-generated; gitignored; outside wwwroot
├── Pages/Login.cshtml.cs                # add Department claim required by sharing rules
├── Program.cs                           # DI registrations + controller mapping
└── appsettings*.json                    # local upload root/configuration

ContosoDashboard.Tests/
├── Services/
│   ├── DocumentServiceTests.cs
│   ├── LocalFileStorageServiceTests.cs
│   └── TrainingFileScanServiceTests.cs
├── Authorization/
│   └── DocumentAuthorizationTests.cs
└── Integration/
    └── DocumentPersistenceTests.cs
```

**Structure Decision**: Keep the existing Blazor Server application as the only production project and add one conventional xUnit test project. New document models, services, pages, and file-delivery controller live alongside existing ContosoDashboard components. No standalone API, frontend, worker, or cloud service is introduced.

## Design Sequence

1. Establish tests and data model first: document entities, relationships, indexes, and clean training-database recreation.
2. Add storage/scanning abstractions and local implementations with cleanup guarantees.
3. Build `DocumentService` around current-user authorization, validation, upload ordering, list/search, and audit behavior.
4. Deliver MVP User Story 1 through the Documents page and navigation, then validate upload/list/download end-to-end.
5. Add browse/search/project views, sharing, audit/reporting, task/dashboard integration in later user-story phases.
6. Add performance and security regression validation before each user story is considered complete.

## Post-Design Constitution Check

Phase 1 design still passes all five constitution principles. File delivery is authorization-gated, local storage remains outside the web root, infrastructure is abstracted, failure cleanup is explicit, and test-first evidence is part of each implementation phase.

## Complexity Tracking

No constitution violations or exceptional complexity are required. The added controller is limited to protected file streaming; all business rules stay in `DocumentService`.