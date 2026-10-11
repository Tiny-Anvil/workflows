# Tiny Anvil workflows

Reusable GitHub Actions workflows for .NET + Vue apps that ship as a container image: build and test gates,
security and licence scans, and a versioned (multi-arch) image build. One fix here reaches every app that calls
them.

They are the shared form of the CI templates in the
[anvil](https://github.com/Tiny-Anvil/agent-skills) agent skills (`github-cicd-dotnet-vue`). Using the skills is not
required.

| Workflow | What it does |
|---|---|
| [`dotnet-build.yml`](.github/workflows/dotnet-build.yml) | restore, Release build, optional "this type must not be in Release" check, `dotnet test`, test results artifact |
| [`frontend-build.yml`](.github/workflows/frontend-build.yml) | pnpm install, lint, unit tests, build, optional "a production build must refuse this flag" check |
| [`security.yml`](.github/workflows/security.yml) | vulnerable NuGet (every project, transitive) and npm packages, CodeQL, Trivy |
| [`license-check.yml`](.github/workflows/license-check.yml) | NuGet and npm licences against an allow-list, report-only or enforced |
| [`container-image.yml`](.github/workflows/container-image.yml) | versioned image from a release, single or multi-arch, to GHCR or another registry; outputs version and digest |

## Quick start

`.github/workflows/ci.yml` in your app:

```yaml
name: CI
on:
  pull_request:
    branches: [main]
  push:
    branches: [main]

permissions:
  contents: read

jobs:
  api:
    uses: Tiny-Anvil/workflows/.github/workflows/dotnet-build.yml@v1
    with:
      project: src/apps/api/MyApp.Api.csproj
      test-project: src/tests/MyApp.Api.Tests/MyApp.Api.Tests.csproj
      forbidden-type: DevBypassAuthenticationHandler   # optional

  spa:
    uses: Tiny-Anvil/workflows/.github/workflows/frontend-build.yml@v1
    with:
      app-dir: src/apps/app
      guard-env: VITE_AUTH_BYPASS                        # optional
      guard-message: VITE_AUTH_BYPASS=true in a production build

  licences:
    uses: Tiny-Anvil/workflows/.github/workflows/license-check.yml@v1
    with:
      dotnet-project: src/apps/api/MyApp.Api.csproj
      enforce: false                                     # report first, enforce after review
```

`.github/workflows/security.yml`:

```yaml
name: Security
on:
  pull_request:
    branches: [main]
  push:
    branches: [main]
  schedule:
    - cron: "17 5 * * 1"      # weekly: new advisories appear for code that did not change

permissions:
  contents: read
  security-events: write       # CodeQL results
  actions: read                # CodeQL on private repos

jobs:
  security:
    uses: Tiny-Anvil/workflows/.github/workflows/security.yml@v1
```

`.github/workflows/image.yml`:

```yaml
name: Image
on:
  release:
    types: [released, prereleased]

permissions:
  contents: read
  packages: write              # push to GHCR with GITHUB_TOKEN

jobs:
  image:
    uses: Tiny-Anvil/workflows/.github/workflows/container-image.yml@v1
    with:
      platforms: linux/amd64,linux/arm64
```

A GitHub release `v1.2.0` builds `ghcr.io/<owner>/<repo>:v1.2.0` (+ `:latest`; a prerelease gets `:dev`), with
`--build-arg VERSION=v1.2.0`. Deploy the version tag, or the `digest` output, never `:latest`.

## Inputs

### dotnet-build

| Input | Default | |
|---|---|---|
| `project` | required | Project built in Release |
| `test-project` | `""` | Test project run in Debug; empty skips tests |
| `global-json` | `global.json` | Pins the SDK |
| `forbidden-type` | `""` | Type name that must not appear in the Release assembly (byte search over the built DLL) |
| `assembly-name` | project file name | Assembly to search, without `.dll` |
| `nuget-source` | `""` | Key of a private source in `nuget.config`, credited with `secrets.nuget-token` |
| `nuget-username` | `token` | Username for that source |

### frontend-build

| Input | Default | |
|---|---|---|
| `app-dir` | required | App folder (repo-relative) |
| `workspace-dir` | `.` | pnpm workspace root (lockfile, `packageManager`) |
| `node-version` | `22` | |
| `lint-script` / `test-script` / `build-script` | `lint` / `test:run` / `build` | `package.json` scripts; empty lint or test skips it |
| `guard-env` | `""` | Env var a production build must refuse when `true` |
| `guard-message` | `""` | Text the refusing build prints (so an unrelated failure does not pass) |

### security

| Input | Default | |
|---|---|---|
| `dotnet` / `npm` / `trivy` | `true` | Turn a scan off |
| `global-json`, `workspace-dir` | `global.json`, `.` | |
| `npm-audit-level` | `high` | |
| `codeql-languages` | `["csharp", "javascript-typescript"]` | JSON array; `[]` skips CodeQL |
| `trivy-severity` | `HIGH,CRITICAL` | |

### license-check

| Input | Default | |
|---|---|---|
| `allowed-licenses` | `.github/allowed-licenses.json` | JSON array of SPDX ids, e.g. `["MIT", "Apache-2.0", "BSD-3-Clause", "ISC"]` |
| `dotnet-project` | `""` | Project or solution; empty skips NuGet |
| `workspace-dir` | `.` | pnpm workspace; empty skips npm |
| `enforce` | `false` | Fail on a licence outside the list |

### container-image

| Input | Default | |
|---|---|---|
| `registry` | `ghcr.io` | |
| `image` | `<registry>/<owner>/<repo>` | Full name without tag |
| `version` | the release tag | `vX.Y.Z[-pre.N]` |
| `platforms` | `linux/amd64` | e.g. `linux/amd64,linux/arm64` |
| `context` / `dockerfile` | `.` / `<context>/Dockerfile` | |
| `push` | `true` | `false` only builds (test a Dockerfile on a PR) |
| `moving-tags` | `true` | `:dev` for a prerelease, `:latest` for a release |

Secrets: `registry-username`, `registry-password` (default: `github.actor` + `GITHUB_TOKEN`, for GHCR).
Outputs: `version`, `image`, `digest`.

For a multi-arch image, let the Dockerfile's build stages run on `$BUILDPLATFORM` and cross-compile
(`dotnet publish -a $TARGETARCH`); QEMU then only emulates the small final stage. See
[`tests/sample/Dockerfile`](tests/sample/Dockerfile). Exclude `**/bin` and `**/obj` in `.dockerignore`, or a local
restore's assets end up in the build and the publish fails for the target platform.

## Versioning

- Releases are `vX.Y.Z`. Publishing a release moves the major tag (`v1`) to it
  ([`update-major-tag.yml`](.github/workflows/update-major-tag.yml)).
- **Pin `@v1`** to get fixes within a major version, or **pin a commit SHA** for full control (Dependabot's
  `github-actions` ecosystem updates SHAs and tags of reusable workflows too).
- Breaking changes (a renamed or removed input, a new required input or permission) only in a new major version.
- Never call these workflows at `@main`.

## Security notes

- A called workflow can never have more permissions than its caller grants. Each workflow above lists what it
  needs; grant exactly that, per job where you can.
- Secrets are passed explicitly; nothing here needs `secrets: inherit`.
- Inputs reach shell scripts only through `env:`, never through `${{ }}` inside a script.
- No `pull_request_target`, no `write-all`.

## Tests

[`ci.yml`](.github/workflows/ci.yml) lints every workflow with actionlint and runs each one against the sample app in
[`tests/sample`](tests/sample) (a minimal .NET API with a test and a Vue SPA), exactly as a caller would; the image
is built for amd64 and arm64 without pushing.

## License

[MIT](LICENSE)
