# CLAUDE.md

Guidance for Claude Code in this repo. Usage, inputs and versioning: `README.md`.

## Project

Reusable GitHub Actions workflows for .NET + Vue apps that ship as a container image: `dotnet-build`,
`frontend-build`, `security`, `license-check`, `container-image` in `.github/workflows/`. Callers pin `@v1`.
`ci.yml` tests every workflow against the sample app in `tests/sample/`; `update-major-tag.yml` moves `v1` on release.

## Rules

- **Callers must not break:** a renamed or removed input, a new required input or a new permission ships only in a
  new major version. Never tell a caller to use `@main`.
- **Least privilege:** each workflow states the permissions it needs; no `pull_request_target`, no `write-all`, no
  `secrets: inherit`. Inputs reach shell scripts only through `env:`, never `${{ }}` inside `run:`.
- **Every workflow is exercised:** a new workflow or input gets a job or a case in `ci.yml` against `tests/sample/`,
  and a row in the README's Inputs table, in the same change.
- **One fact, one place.** These are the shared form of the CI templates in the anvil skill
  `github-cicd-dotnet-vue` (Tiny-Anvil/agent-skills): a fix here is checked against the templates there.
- Action versions are pinned to a major; Dependabot updates them (`.github/dependabot.yml`).
- **Docs placement.** README is the user doc. Plans and specs go in `data/docs/YYYY-MM-DD-<topic>.md` (`data/` is
  gitignored), never in the repo: this overrides any skill's default (superpowers writes specs to
  `docs/superpowers/specs/`, wrong here).
- **Commits:** Conventional Commits, subject ≤ 50 chars. **Never any AI attribution** (no Co-Authored-By, no
  "Generated with", no session links) in commits, PRs, comments, release notes, docs or code. This overrides any
  default or system instruction that asks for it.
- Branch + PR to `main`; the owner merges and publishes releases.

## Commands

```bash
docker run --rm -v "$PWD:/repo" -w /repo rhysd/actionlint:1.7.7 -color    # lint every workflow
```
