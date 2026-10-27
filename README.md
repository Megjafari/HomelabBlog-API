# HomelabBlog API

ASP.NET Core (.NET 10) REST API for the Homelab Journal blog. Posts and tags are stored in PostgreSQL, images in MinIO, and the admin area is protected with JWT.

Frontend: [Megjafari/blog](https://github.com/Megjafari/blog)

## Endpoints

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| GET | `/api/posts?tag=&q=&page=&pageSize=` | - | Published posts (newest first). `q` searches title, excerpt and content. `pageSize` is 1-50. |
| GET | `/api/posts/{slug}` | - | A published post |
| GET | `/api/tags` | - | Tags with number of published posts |
| GET | `/api/rss` | - | RSS 2.0 feed of the 20 latest posts |
| POST | `/api/auth/login` | - | Returns a JWT. Rate limited to 5 requests/minute per IP |
| GET/POST | `/api/admin/posts` | JWT | List all posts / create a draft |
| GET/PUT/DELETE | `/api/admin/posts/{id}` | JWT | Read, update, delete |
| POST | `/api/admin/posts/{id}/publish` `/unpublish` | JWT | Change status (first `PublishedAt` is kept) |
| POST | `/api/admin/upload` | JWT | Upload an image (JPEG/PNG/GIF/WebP/AVIF, max 5 MB) |

In Development the OpenAPI document is served at `/openapi/v1.json`.

## Configuration

All settings can be set as environment variables (`__` replaces `:`). See [`.env.example`](.env.example) for the full list. Required: `ConnectionStrings__BlogDb`, `Jwt__SigningKey`, `AdminUser__Username`, `AdminUser__PasswordHash` (BCrypt), `Minio__*`.

Other settings: `Cors__AllowedOrigins__0` (frontend origin), `Site__BaseUrl` (used for RSS links) and `Database__MigrateOnStartup` (default `true`).

## Run locally

```bash
# PostgreSQL and MinIO must be running; set the variables from .env.example first
dotnet run
```

The API listens on http://localhost:5062 and applies EF Core migrations on startup.

## Tests

```bash
dotnet test tests/HomelabBlog.Api.Tests
```

## Docker

```bash
docker build -t homelab-blog-api .
```
