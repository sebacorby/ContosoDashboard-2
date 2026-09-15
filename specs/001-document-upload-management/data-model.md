# Data Model: Document Upload and Management

**Feature**: `001-document-upload-management`  
**Date**: 2026-09-15

## Document

Represents one uploaded work document and its business metadata.

| Field | Type | Rules |
|---|---|---|
| DocumentId | int | Primary key; integer identity |
| Title | string | Required; user-facing title |
| Description | string? | Optional |
| Category | string | Required; one of Project Documents, Team Resources, Personal Files, Reports, Presentations, Other |
| OriginalFileName | string | Required; preserved for display/download |
| StoragePath | string | Required; relative, generated, unique, never derived directly from user path input |
| FileSize | long | Required; >0 and <= 25 MB |
| FileType | string | Required; max 255 characters |
| UploadedByUserId | int | Required FK to User |
| ProjectId | int? | Optional FK to Project |
| UploadDate | DateTime | Required UTC timestamp |

**Indexes**: `UploadedByUserId`, `ProjectId`, `Category`, `UploadDate`; unique index on `StoragePath`.

**Deletion rule**: Document deletion removes shares, tags, task links, and stored file content. Audit activity remains available through a snapshot reference.
## DocumentTag

Represents a searchable custom tag assigned to a document.

| Field | Type | Rules |
|---|---|---|
| DocumentTagId | int | Primary key |
| DocumentId | int | Required FK to Document; cascade on document deletion |
| Value | string | Required; trimmed; case-insensitive comparison |

**Constraint**: unique `(DocumentId, Value)` combination.

## DocumentShare

Represents explicit sharing outside normal ownership. Project-associated documents still require project authorization and sharing cannot bypass project membership.

| Field | Type | Rules |
|---|---|---|
| DocumentShareId | int | Primary key |
| DocumentId | int | Required FK to Document; cascade on document deletion |
| SharedByUserId | int | Required FK to User |
| SharedWithUserId | int? | Optional user target |
| SharedWithDepartment | string? | Optional team/department target |
| SharedDate | DateTime | Required UTC timestamp |

**Constraint**: exactly one share target must be populated. Duplicate active shares to the same target are not allowed.

## DocumentTask

Represents a many-to-many association between documents and existing tasks.

| Field | Type | Rules |
|---|---|---|
| DocumentTaskId | int | Primary key |
| DocumentId | int | Required FK to Document; cascade on document deletion |
| TaskId | int | Required FK to TaskItem |

**Constraint**: unique `(DocumentId, TaskId)` combination. A task-linked document must belong to the task's project.## DocumentActivity

Represents an immutable audit event for document actions.

| Field | Type | Rules |
|---|---|---|
| DocumentActivityId | int | Primary key |
| DocumentId | int? | Nullable logical reference so audit survives document deletion |
| DocumentTitleSnapshot | string | Required snapshot for deleted-document reporting |
| UserId | int | Required actor FK to User |
| Action | string | Upload, Download, Preview, Share, MetadataEdit, Replace, Delete, Attach |
| OccurredAt | DateTime | Required UTC timestamp |
| Details | string? | Optional concise event context |

## Authorization Model

A user may access a document when at least one rule is true:

1. The user is an Administrator.
2. The user uploaded a personal document.
3. The document belongs to a project and the user is that project's manager or current member.
4. The document is personal/non-project content explicitly shared to the user or their department.

Explicit sharing MUST NOT bypass project membership for project-associated documents.

## Lifecycle and State Transitions

- **Upload**: Validate metadata and authorization → validate type/size → scan → persist file → persist metadata/tags/activity.
- **Upload failure**: Delete partial/stored content and do not retain a completed Document record.
- **Replace file**: Validate/scan new content → store replacement → update metadata/path → remove superseded content; no version history retained.
- **Member removed**: Access derived from project membership ends immediately; project document remains.
- **Share deleted/revoked**: Recipient access ends immediately unless another authorization rule still grants access.
- **Document deleted**: File and dependent metadata are permanently removed after confirmation; audit record is preserved.
- **Project deleted**: Associated document files and metadata are permanently removed as part of confirmed project deletion, with audit entries.

## Validation Rules

- Maximum file size: 25 MB.
- Allowed types: PDF, Word, Excel, PowerPoint, text, JPEG, PNG.
- MIME type field supports up to 255 characters.
- Category is stored as text and restricted to the six stakeholder-defined labels.
- User-supplied filenames never determine the storage directory or storage identity.
- `StoragePath` is relative and unique; uploaded files live outside `wwwroot`.