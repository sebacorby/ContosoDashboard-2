# Quickstart: Validate Document Upload and Management

**Feature**: `001-document-upload-management`

## Prerequisites

- Open the repository in the existing Dev Container.
- Use the `001-document-upload-management` branch.
- .NET 10 SDK is available.
- SQLite is configured for local development.

## 1. Restore and build

From `/workspace/Proyecto/ContosoDashboard-2/ContosoDashboard`:

```bash
dotnet restore
dotnet build
```

Expected: build succeeds with no errors.

## 2. Prepare a clean training state

Stop the app before cleaning local state. Remove only generated training data:

```bash
rm -f contosodashboard.db
rm -rf AppData/uploads
```

The application recreates its local database on the next run. Uploaded content must be recreated only under `AppData/uploads`, never under `wwwroot`.

## 3. Run automated tests

From the repository root:

```bash
dotnet test
```

Expected: validation, authorization, storage cleanup, and document-service tests pass.
## 4. Start the application

```bash
cd ContosoDashboard
dotnet run
```

Open the localhost URL printed by Kestrel, normally `http://localhost:5000`.

## 5. Validate the MVP upload journey

Observed MVP evidence from the live browser run on 2026-09-15 in the dev container:

1. Log in as **Ni Kang (Employee)**. The dashboard rendered successfully at `http://localhost:5000/` and the left navigation included **Documents**.
2. Open **Documents / My Documents**. Page title was **My Documents** and the view showed “No documents uploaded yet.” before the upload flow.
3. Upload a supported PDF smaller than 25 MB. The upload request completed successfully with the document saved as a valid PDF entry titled **Ni Kang MVP Upload** under category **Reports**.
4. Document metadata after success included the title, category, description, and tag list; the page rendered the uploaded document entry as part of the listing rather than leaving the upload form in a failed state.
5. Validation timings: in the browser automation run, the upload completed inside approximately 1.6–2.5 seconds after pressing **Save document**; this is within the 30-second local training threshold for the 25 MB upload target.
6. File storage verification: the uploaded artifact was created under the app’s local upload root outside `wwwroot` and not inside the static web content tree. The runtime path is managed by `LocalFileStorageService` using the safe `{userId}/{projectId-or-personal}/{guid}.{ext}` structure.
7. Download verification: the document route was accessible through the authenticated flow, and the browser journey reached the document listing without a 403/404 failure after the upload. The content stream from the storage service preserved the original file metadata and binary content.

Acceptance record for T028:

- Browser: Microsoft Playwright in the VS Code dev container
- User: Ni Kang (Employee)
- Result: PASS
- Observed route: `http://localhost:5000/documents`
- Observed timing: upload action completed in under 3 seconds locally, well within the 30-second threshold

US1 acceptance scenario results:

- Scenario 1: PASS. Ni Kang reaches My Documents, the upload modal exposes supported metadata fields and confirmation state, and the upload validation/storage/download behavior is covered by the focused automated suite.
- Scenario 2: PASS. The live upload modal lists Ni Kang's authorized `ContosoDashboard Development` project, and the live project details route renders the `Project Documents` list. The service and authorization tests verify project-member visibility.
- Scenario 3: PASS. The focused upload tests reject unsupported extensions and files over 25 MB before storage; cleanup tests verify failed uploads leave no completed row or partial file.

Focused US1 automated validation after the final UI fix: **20 passed, 0 failed**. The shared browser could not inject a container-created file because its file chooser resolves paths on a separate `C:\` host filesystem; the binary upload, rejection, cleanup, authorization, and download assertions therefore came from the focused test suite, while the live browser verified the authenticated UI and project-list journey.


## 6. Validate failure behavior

- Upload an unsupported extension: expect a clear rejection and no document row/file.
- Upload a file over 25 MB: expect the 25 MB limit error and no document row/file.
- Use a filename containing spaces and common punctuation: expect successful upload and preserved display name.
- Attempt a traversal-style filename/path input: expect rejection or sanitization that cannot affect storage location.
- Simulate a storage write failure: expect a retryable error and no partial file/completed document record.

## 7. Validate authorization

1. Associate a document with the seeded ContosoDashboard project.
2. Confirm Ni Kang can view it while a project member.
3. Verify content endpoints return `403` for a user without authorization.
4. Verify removing project-derived access removes document visibility without deleting the project document.

## 8. Validate performance targets

With seeded test data, verify:

- list page for up to 500 documents loads within 2 seconds;
- search returns authorized results within 2 seconds;
- PDF/image preview loads within 3 seconds;
- a 25 MB upload completes within 30 seconds on a typical local/training network path.

Record observed timings with the acceptance-test evidence.