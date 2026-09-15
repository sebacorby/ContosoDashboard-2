# Contract: File Storage and Scan Services

## `IFileStorageService`

The business layer depends on a storage abstraction rather than direct filesystem access.

```csharp
Task<string> UploadAsync(Stream content, string originalFileName, int userId, int? projectId, CancellationToken ct);
Task<Stream> DownloadAsync(string storagePath, CancellationToken ct);
Task DeleteAsync(string storagePath, CancellationToken ct);
Task<bool> ExistsAsync(string storagePath, CancellationToken ct);
```

`UploadAsync` returns a relative storage path. Implementations must generate safe unique storage identities and never trust a user-supplied path.

## `IFileScanService`

```csharp
Task<FileScanResult> ScanAsync(Stream content, string originalFileName, string contentType, CancellationToken ct);
```

`FileScanResult` communicates `IsSafe` and a user-safe rejection reason. The training implementation must be deterministic and explicitly training-grade; production malware scanning is outside this lab.

## `IDocumentService`

The document business service owns validation, authorization, persistence ordering, cleanup, search/filter operations, and audit logging. UI components and file endpoints must call this service rather than query document tables directly.

## Failure contract

- Validation or scan failure: no file or document row is persisted.
- Storage write failure: no completed document row is created.
- Metadata persistence failure after storage: the stored file is deleted before the error is returned.
- Delete failure: the operation reports failure without falsely reporting a successful deletion.
