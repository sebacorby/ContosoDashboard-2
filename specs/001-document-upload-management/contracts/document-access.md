# Contract: Document Access and Storage

**Feature**: `001-document-upload-management`

## UI routes

- `/documents` — authenticated user's My Documents view; lists owned, project-authorized, and explicitly shared documents.
- `/projects/{projectId}` — existing project detail view extended with project documents for authorized members.
- `/tasks/{taskId}` or the existing task detail surface — displays and attaches documents associated with the task's project.

## File content endpoints

Uploaded content is never served as a static file. Content retrieval must pass through an authenticated endpoint.

### `GET /api/documents/{documentId}/download`

**Success**: `200 OK` with original filename in `Content-Disposition` and stored MIME type.  
**Unauthorized/forbidden**: `403 Forbidden`.  
**Missing document or content**: `404 Not Found`.

### `GET /api/documents/{documentId}/preview`

Available only for preview-supported types such as PDF, JPEG, and PNG.

**Success**: `200 OK` with inline content disposition.  
**Unsupported preview type**: `415 Unsupported Media Type`.  
**Unauthorized/forbidden**: `403 Forbidden`.  
**Missing document or content**: `404 Not Found`.
