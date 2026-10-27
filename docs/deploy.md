# Deploying to Coolify

Target setup: Coolify in one Proxmox VM, PostgreSQL in another VM.

| What | Where |
| --- | --- |
| Frontend (nginx) | https://blog.meghdadjafari.dev |
| API (ASP.NET Core) | https://blog-api.meghdadjafari.dev |
| Database | PostgreSQL on its own VM |
| Images | Volume on the API container, served from `/uploads` |

The frontend calls the API on its own subdomain, so the API must allow the frontend origin (CORS) and the frontend is built with the API URL.

## 1. DNS

Point both names at your Coolify entry point (A/CNAME records, or the same Cloudflare Tunnel):

- `blog.meghdadjafari.dev`
- `blog-api.meghdadjafari.dev`

## 2. PostgreSQL (on the database VM)

```sql
CREATE USER blog WITH PASSWORD 'choose-a-strong-password';
CREATE DATABASE blog OWNER blog;
```

Make sure Postgres accepts connections from the Coolify VM:

- `postgresql.conf`: `listen_addresses = '*'` (or the VM's LAN address)
- `pg_hba.conf`: `host  blog  blog  <coolify-vm-ip>/32  scram-sha-256`
- allow port 5432 from the Coolify VM only (firewall)

Then reload Postgres. The API creates the tables itself on startup (EF Core migrations).

## 3. Admin password hash

The API stores a BCrypt hash, never the password. Generate one (replace the password):

```bash
docker run --rm httpd:2-alpine htpasswd -nbBC 10 x 'your-password' | cut -d: -f2
```

The output starts with `$2y$`. Use it as `AdminUser__PasswordHash`. If you paste it into a Coolify environment variable and it contains `$`, check that it is saved unchanged (no variable expansion).

Generate the JWT signing key with e.g. `openssl rand -base64 48`.

## 4. API application in Coolify

- Source: this repository, branch `main`, build pack **Dockerfile**
- Domain: `https://blog-api.meghdadjafari.dev`
- Port: **8080**
- Health check path: `/health`
- Persistent storage: a volume mounted at **`/data/uploads`** (this is where uploaded images live; back it up)
- Environment variables: see [`.env.example`](../.env.example). Required:
  - `ConnectionStrings__BlogDb`
  - `Jwt__SigningKey`
  - `AdminUser__Username`, `AdminUser__PasswordHash`
  - `Cors__AllowedOrigins__0=https://blog.meghdadjafari.dev`
  - `Site__BaseUrl=https://blog.meghdadjafari.dev`
  - `Storage__PublicBaseUrl=https://blog-api.meghdadjafari.dev/uploads`

Check: `https://blog-api.meghdadjafari.dev/health` returns `Healthy`, and `/api/posts` returns `[]`.

## 5. Frontend application in Coolify

Repository: [Megjafari/blog](https://github.com/Megjafari/blog)

- Build pack **Dockerfile**, domain `https://blog.meghdadjafari.dev`, port **80**
- Build argument: `VITE_API_BASE_URL=https://blog-api.meghdadjafari.dev` (it is baked into the bundle, so changing it needs a rebuild)

## 6. First login

Open `https://blog.meghdadjafari.dev/admin/login`, log in, write a post and publish it. Check that:

- the post shows on `/blog`
- an uploaded image still loads after you redeploy the API (the volume is working)
- `https://blog-api.meghdadjafari.dev/api/rss` returns the feed

## Backups

- Database: `pg_dump -U blog blog > blog-$(date +%F).sql` from a cron job on the Postgres VM (or a Coolify scheduled backup if you manage that database from Coolify).
- Images: back up the `/data/uploads` volume.

## Using MinIO instead of local storage

Set `Storage__Provider=Minio` and the `Minio__*` variables from `.env.example`. Local storage is the default because it needs no extra service.
