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

1. Log in as **Ni Kang (Employee)**.
2. Open **Documents / My Documents**.
3. Upload a supported PDF smaller than 25 MB.
4. Enter Title `Test Document` and Category `Personal Files`.
5. Confirm an upload progress indicator appears.
6. Confirm success feedback appears and the document is listed with title, category, upload date, file size, and project/personal classification.
7. Download the document and verify the original filename/content are preserved.
8. Verify the stored file exists under `AppData/uploads` and no uploaded content exists under `wwwroot`.

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