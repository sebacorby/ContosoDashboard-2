# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: Required by the project constitution. For behavior changes, write tests first and confirm they fail before implementation.

**Organization**: Tasks are grouped by user story and use exact repository paths. The MVP is Setup + Foundational + User Story 1.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare repeatable test/build infrastructure and runtime-safe local storage configuration.

- [X] T001 Create `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj` as an xUnit .NET 10 test project referencing `ContosoDashboard/ContosoDashboard.csproj`
- [X] T002 Create `ContosoDashboard.sln` and add `ContosoDashboard/ContosoDashboard.csproj` plus `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj`
- [X] T003 [P] Ignore generated SQLite databases and runtime upload content in `.gitignore` for `ContosoDashboard/*.db` and `ContosoDashboard/AppData/uploads/`
- [X] T004 [P] Add local document-storage root and 25 MB upload-limit configuration in `ContosoDashboard/appsettings.json` and `ContosoDashboard/appsettings.Development.json`

**Checkpoint**: `dotnet restore ContosoDashboard.sln` and an empty `dotnet test ContosoDashboard.sln` are runnable.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish shared data, authorization, storage, scanning, and service infrastructure before user-story implementation.

**CRITICAL**: No user-story work begins until this phase is complete.
- [X] T005 [P] Create `Document` with integer `DocumentId`, required title/category/original filename/storage path/file size/file type/uploader/upload date and optional project/description fields in `ContosoDashboard/Models/Document.cs`
- [X] T006 [P] Create `DocumentTag` with required trimmed tag value and document relationship in `ContosoDashboard/Models/DocumentTag.cs`
- [X] T007 [P] Create `DocumentShare` with exactly one user-or-department target and sharing metadata in `ContosoDashboard/Models/DocumentShare.cs`
- [X] T008 [P] Create `DocumentTask` with unique document/task relationship in `ContosoDashboard/Models/DocumentTask.cs`
- [X] T009 [P] Create immutable `DocumentActivity` audit model with nullable logical document reference and title snapshot in `ContosoDashboard/Models/DocumentActivity.cs`
- [X] T010 Configure document DbSets, relationships, delete behaviors, `StoragePath` uniqueness, MIME length 255, and document indexes in `ContosoDashboard/Data/ApplicationDbContext.cs`
- [X] T011 [P] Write failing safe-path, upload/delete, and partial-cleanup tests for local storage in `ContosoDashboard.Tests/Services/LocalFileStorageServiceTests.cs`
- [X] T012 [P] Write failing safe/unsafe scan-result tests in `ContosoDashboard.Tests/Services/TrainingFileScanServiceTests.cs`
- [X] T013 [P] Define `IFileStorageService` and storage-path contract in `ContosoDashboard/Services/IFileStorageService.cs`
- [X] T014 [P] Define `IFileScanService` and `FileScanResult` contract in `ContosoDashboard/Services/IFileScanService.cs`
- [X] T015 Implement deterministic training-grade scanning behavior in `ContosoDashboard/Services/TrainingFileScanService.cs` until T012 passes
- [X] T016 Implement generated `{userId}/{projectId-or-personal}/{guid}.{ext}` paths, safe writes/download/delete, and cleanup in `ContosoDashboard/Services/LocalFileStorageService.cs` until T011 passes
- [X] T017 Define authorization-aware document operations in `ContosoDashboard/Services/IDocumentService.cs`
- [X] T018 Add the required `Department` authentication claim in `ContosoDashboard/Pages/Login.cshtml.cs`

**Checkpoint**: Shared schema and infrastructure compile; storage/scanner tests pass; user stories may begin.

---

## Phase 3: User Story 1 - Upload and organize work documents (Priority: P1) — MVP

**Goal**: An authenticated employee can upload a supported document, categorize it as personal or project-related, see upload feedback, browse the resulting metadata, and securely download the stored file.

**Independent Test**: Log in as Ni Kang, upload a supported PDF under 25 MB with title/category, verify progress/success/list metadata, then download the same content while confirming it is stored outside `wwwroot`.

### Tests for User Story 1 — write first and confirm RED
- [X] T019 [P] [US1] Write failing tests for required title/category, supported PDF/Office/text/JPEG/PNG types, and the 25 MB maximum in `ContosoDashboard.Tests/Services/DocumentServiceUploadTests.cs`
- [X] T020 [P] [US1] Write failing tests proving scan/storage/database failures leave no completed document row or partial file in `ContosoDashboard.Tests/Services/DocumentUploadCleanupTests.cs`
- [X] T021 [P] [US1] Write failing owner/project-member visibility and unauthorized-access tests in `ContosoDashboard.Tests/Authorization/DocumentAuthorizationTests.cs`
- [X] T022 [P] [US1] Write failing download contract tests for 200/403/404 behavior and original filename preservation in `ContosoDashboard.Tests/Integration/DocumentsControllerTests.cs`

### Implementation for User Story 1

- [X] T023 [US1] Implement upload validation, scan→store→metadata ordering, tags, upload audit, cleanup, and My Documents/project visibility queries in `ContosoDashboard/Services/DocumentService.cs` until T019–T021 pass
- [X] T024 [US1] Implement authorization-gated download endpoint and original filename/MIME response in `ContosoDashboard/Controllers/DocumentsController.cs` until T022 passes
- [X] T025 [US1] Implement My Documents table, upload modal, required metadata validation, `InputFile` reset, MemoryStream copy, progress state, and success/error feedback in `ContosoDashboard/Pages/Documents.razor`
- [X] T026 [P] [US1] Add the Documents navigation entry in `ContosoDashboard/Shared/NavMenu.razor`
- [X] T027 [US1] Register document/storage/scanner services, controller support, and endpoint mapping in `ContosoDashboard/Program.cs`
- [X] T028 [US1] Execute the User Story 1 build/tests and Ni Kang browser journey, then record observed MVP and timing evidence in `specs/001-document-upload-management/quickstart.md`

**Checkpoint / MVP STOP**: Tasks T001–T028 deliver and validate User Story 1 independently. Do not begin US2 until the MVP is green.

---

## Phase 4: User Story 2 - Browse, search, and review project documents (Priority: P2)

**Goal**: Authorized users can efficiently sort/filter/search their documents, browse project documents, and preview supported content without exposing unauthorized results.

**Independent Test**: Seed up to 500 documents, search/filter as multiple users, verify only authorized matches appear within 2 seconds, and preview PDF/images within 3 seconds.

### Tests for User Story 2 — write first and confirm RED

- [ ] T029 [P] [US2] Write failing sort/filter/search tests covering title, description, tags, uploader, project, category, date range, and authorization in `ContosoDashboard.Tests/Services/DocumentSearchTests.cs`
- [ ] T030 [P] [US2] Write failing preview endpoint tests for supported PDF/JPEG/PNG, 403, 404, and 415 outcomes in `ContosoDashboard.Tests/Integration/DocumentsPreviewControllerTests.cs`
- [ ] T031 [P] [US2] Write failing performance acceptance tests for 500-document list and authorized search targets in `ContosoDashboard.Tests/Performance/DocumentQueryPerformanceTests.cs`
### Implementation for User Story 2

- [ ] T032 [US2] Implement authorized sorting/filtering/search projections and query limits in `ContosoDashboard/Services/DocumentService.cs` until T029 and T031 pass
- [ ] T033 [US2] Add PDF/JPEG/PNG authorization-gated inline preview handling in `ContosoDashboard/Controllers/DocumentsController.cs` until T030 passes
- [ ] T034 [US2] Add search, category/project/date filters, sorting controls, empty/loading states, and preview actions in `ContosoDashboard/Pages/Documents.razor`
- [ ] T035 [US2] Add authorized project-document listing and project-manager upload entry point in `ContosoDashboard/Pages/ProjectDetails.razor`
- [ ] T036 [US2] Tune EF Core document indexes/query shapes needed to meet list/search targets in `ContosoDashboard/Data/ApplicationDbContext.cs`
- [ ] T037 [US2] Execute US2 automated and browser performance scenarios and record list/search/preview timings in `specs/001-document-upload-management/quickstart.md`

**Checkpoint**: US1 and US2 remain independently testable and all authorization/performance tests are green.

---

## Phase 5: User Story 3 - Share, manage access, and maintain accountability (Priority: P3)

**Goal**: Owners and authorized project leaders can edit, replace, share, delete, and audit documents while recipients receive notifications and access is revoked correctly.

**Independent Test**: Share a personal document, confirm recipient notification/visibility, edit/replace it, then delete it and verify access revocation plus preserved audit evidence.

### Tests for User Story 3 — write first and confirm RED

- [ ] T038 [P] [US3] Write failing edit/replace/delete authorization, confirmation, cleanup, and share-revocation tests in `ContosoDashboard.Tests/Services/DocumentManagementTests.cs`
- [ ] T039 [P] [US3] Write failing user/department sharing, project-share restriction, and recipient notification tests in `ContosoDashboard.Tests/Services/DocumentSharingTests.cs`
- [ ] T040 [P] [US3] Write failing immutable audit-event and administrator-report aggregation tests in `ContosoDashboard.Tests/Services/DocumentAuditTests.cs`
- [ ] T041 [P] [US3] Write failing project-member removal/project-deletion document lifecycle tests in `ContosoDashboard.Tests/Integration/DocumentLifecycleTests.cs`

### Implementation for User Story 3
- [ ] T042 [US3] Implement owner/project-manager metadata edit, validated file replacement, permanent delete, share revocation, and audit behavior in `ContosoDashboard/Services/DocumentService.cs` until T038 and T040 pass
- [ ] T043 [US3] Implement user/department shares with project-membership enforcement in `ContosoDashboard/Services/DocumentService.cs` until T039 passes
- [ ] T044 [US3] Integrate document share/project-document notifications through `ContosoDashboard/Services/NotificationService.cs`
- [ ] T045 [US3] Add metadata edit, replacement, delete confirmation, share controls, and Shared with Me view to `ContosoDashboard/Pages/Documents.razor`
- [ ] T046 [US3] Add administrator document-activity reporting for uploaded types, active uploaders, and access patterns in `ContosoDashboard/Pages/DocumentReports.razor`
- [ ] T047 [US3] Enforce clarified member-removal/project-deletion document lifecycle cleanup in `ContosoDashboard/Services/ProjectService.cs` until T041 passes
- [ ] T048 [US3] Execute US3 share/manage/audit browser scenarios and record evidence in `specs/001-document-upload-management/quickstart.md`

**Checkpoint**: All three user stories are independently functional and regression tests remain green.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Complete feature integrations and whole-feature quality gates that span user stories.

- [ ] T049 [P] Add task-document attachment/display integration with project-consistency enforcement in `ContosoDashboard/Pages/Tasks.razor`
- [ ] T050 [P] Add Recent Documents (last 5 uploaded by current user) and document-count dashboard integration in `ContosoDashboard/Pages/Index.razor`
- [ ] T051 Add regression tests for task/dashboard document integrations in `ContosoDashboard.Tests/Integration/DocumentFeatureIntegrationTests.cs`
- [ ] T052 Run full security review for IDOR, traversal, unauthorized preview/download, project-share bypass, and stored-file exposure and encode regressions in `ContosoDashboard.Tests/Authorization/DocumentSecurityRegressionTests.cs`
- [ ] T053 Run `dotnet test ContosoDashboard.sln`, full `quickstart.md` browser validation, and all four performance targets; record final observed evidence in `specs/001-document-upload-management/quickstart.md`
- [ ] T054 Update feature usage, offline-storage limitations, and training-grade scanner disclaimer in `README.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup (T001–T004)**: no dependencies.
- **Phase 2 Foundational (T005–T018)**: depends on Setup and blocks all user stories.
- **Phase 3 US1 MVP (T019–T028)**: depends on Foundational only.
- **Phase 4 US2 (T029–T037)**: depends on Foundational and uses the document infrastructure delivered by US1; its search/preview acceptance remains independently testable.
- **Phase 5 US3 (T038–T048)**: depends on Foundational and the core document service; sharing/management/audit acceptance remains independently testable.
- **Phase 6 Polish (T049–T054)**: follows the desired story set and validates cross-cutting integrations.
### User Story Dependencies

- **US1 (P1)**: starts after T001–T018; no dependency on US2/US3 and defines the MVP.
- **US2 (P2)**: starts after foundation and reuses shared DocumentService/storage delivered by US1; can be validated independently with seeded document data.
- **US3 (P3)**: starts after foundation and shared document infrastructure; does not require US2 search/preview behavior for its own acceptance test.

### Within Each Story

1. Execute test tasks and confirm RED.
2. Implement models/services/endpoints needed for the story.
3. Run focused tests until GREEN and refactor without changing behavior.
4. Run the story's independent browser/quickstart validation.
5. Do not mark the phase complete while any story regression is red.

### Parallel Opportunities

- T003 and T004 can run in parallel after test-project setup decisions are known.
- T005–T009 are independent model files and can be created in parallel.
- T011/T012 and T013/T014 can run in parallel before the corresponding implementations.
- T019–T022 are independent US1 test files and can be written in parallel.
- T029–T031 are independent US2 test files and can be written in parallel.
- T038–T041 are independent US3 test files and can be written in parallel.
- T049 and T050 affect separate existing pages and can run in parallel after the story set is stable.

## Parallel Example: User Story 1

```text
T019: DocumentServiceUploadTests.cs
T020: DocumentUploadCleanupTests.cs
T021: DocumentAuthorizationTests.cs
T022: DocumentsControllerTests.cs
```

After those tests are demonstrably RED, T023/T024 can be implemented on separate service/controller files before UI integration T025–T027.

---

## Requirement Traceability


| Requirement | Primary Tasks |
|---|---|
| FR-001–FR-005 | T019, T023, T025 |
| FR-006 | T012, T014, T015, T023 |
| FR-007 | T011, T013, T016, T021, T023–T024 |
| FR-008–FR-010 | T023, T029, T032, T034–T037 |
| FR-011 | T022, T024, T030, T033–T034 |
| FR-012–FR-013 | T038, T042, T045 |
| FR-014–FR-015 | T039, T043–T045 |
| FR-016 | T050–T051 |
| FR-017 | T049, T051 |
| FR-018–FR-019 | T009, T040, T042, T046 |
| FR-020 | T003–T004, T013, T016, T027, T054 |
| FR-021 | T005–T010 |
| FR-022–FR-023 | T041, T047 |
| FR-024 | T038, T042, T045 |
| FR-025 | T011, T016, T022, T024 |
| FR-026 | T011, T020, T016, T023 |
| FR-027 | T021, T039, T043, T052 |

## Implementation Strategy

### MVP First — T001–T028
1. Complete Setup T001–T004 and verify restore/test entry points.
2. Complete Foundational T005–T018 with storage/scanner tests RED→GREEN.
3. Execute US1 tests T019–T022 and confirm RED before implementation.
4. Implement T023–T027 until focused tests pass.
5. Execute T028 and stop: validate Ni Kang upload/list/download flow independently.
6. Do not start T029 until the MVP checkpoint is green.

### Incremental Delivery

1. MVP: T001–T028 → upload/organize/download.
2. Increment 2: T029–T037 → browse/search/preview with performance gates.
3. Increment 3: T038–T048 → manage/share/audit lifecycle.
4. Final integration: T049–T054 → task/dashboard integration, security regression, full quickstart.

### Definition of Done per Phase

- All phase tasks are checked in `tasks.md`.
- Required RED evidence exists before behavior implementation tasks.
- Focused and regression tests are GREEN.
- No authorization or storage-path regression is introduced.
- The phase's independent browser scenario is reproducible.
- Build/test evidence is recorded in `quickstart.md` at story checkpoints.

## Notes

- `[P]` means the task can execute concurrently without editing the same incomplete dependency.
- `[US1]`, `[US2]`, `[US3]` map directly to prioritized user stories in `spec.md`.
- T001–T028 is the lab's explicit MVP implementation range.
- Commit at story/checkpoint boundaries so failures can be isolated and reviewed.
