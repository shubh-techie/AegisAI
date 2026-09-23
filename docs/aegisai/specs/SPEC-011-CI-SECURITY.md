# SPEC-011 — GitHub CI and repository security baseline

Date: 2026-09-23  
Task: AEGISAI-011  
Status: **IMPLEMENTED** in working tree; pending review, not committed.

## Workflow contract

[CI](../../../.github/workflows/ci.yml) runs on pull requests targeting main,
including drafts, without path filters. The default pull_request activities cover
opening, reopening, and updating a PR. Checkout uses the event's default merge
revision. One GitHub-hosted ubuntu-24.04 job, named Build and test, runs:

1. Checkout repository.
2. Install the latest stable .NET 8 SDK using 8.0.x, compatible with the existing
   global.json latestFeature roll-forward policy.
3. dotnet restore AegisAI.sln.
4. dotnet build AegisAI.sln --configuration Release --no-restore.
5. dotnet test AegisAI.sln --configuration Release --no-build --no-restore.

All eleven solution projects build, including experiments and development tools;
all five test projects run without test filters. Separate steps propagate nonzero
exit codes and use default success gating. No continue-on-error or shell failure
suppression is used: restore, build, or test failure fails the job/workflow.
The job times out after 20 minutes. No application behavior changes are included.

## Permissions and credential handling

The workflow grants only contents: read; unspecified configurable token permissions
are disabled. Checkout sets persist-credentials: false. No repository secrets,
environment secrets, PATs, SSH keys, deployment environments, or OIDC write access
are requested. Tests generate synthetic credentials locally. There are no uploads,
deployment steps, environment dumps, shared caches, or privileged PR triggers.
Use pull_request, not pull_request_target, to execute contributed build/test code.

Actions are pinned to full upstream commit hashes, verified with git ls-remote:

| Action | Upstream tag checked | Commit |
| --- | --- | --- |
| actions/checkout | v6 | d23441a48e516b6c34aea4fa41551a30e30af803 |
| actions/setup-dotnet | v5 | 26b0ec14cb23fa6904739307f278c14f94c95bf1 |

Pins require reviewed updates. The SDK and hosted runner receive updates, so this
does not promise byte-identical builds. See the official
[workflow syntax](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax),
[checkout](https://github.com/actions/checkout), and
[setup-dotnet](https://github.com/actions/setup-dotnet) documentation.

## Security review and observations

Reviewed .gitignore, .dockerignore, global.json, AegisAI.sln, all project files,
appsettings.example.json, Dockerfile, Compose, ./dev, tools-entrypoint.sh, and the
DevTools credential generator. Also scanned current tracked text for private-key
blocks, common provider tokens, JWT literals, credential assignments, and URLs
with embedded credentials; reported locations only, never potential values.

- No accidental credential literals or sensitive production values were found
  in the reviewed current files. No tracked secret-file candidates were found.
  Example identity URLs use .invalid and policy/context values are synthetic.
- .gitignore already excludes local environment/configuration, key files, secrets,
  build outputs, and deployment/docker/.local. Twelve representative secret paths
  were verified with git check-ignore; .gitignore required no changes.
- Added **/*.jwk and **/*.secrets.json to .dockerignore to match existing Git
  exclusions. Other local credentials and results were already excluded. Ignore
  patterns do not protect secrets placed in arbitrary files or force-added to Git.
- Docker configuration has no credential build arguments or literal secrets.
  The API runs non-root with loopback publication, a read-only root filesystem,
  dropped capabilities, and public local trust mounted read-only.
- DevTools generates a one-hour synthetic token, discards its private key, writes
  the token with owner-only final permissions, and does not print it. ./dev down
  leaves generated local files in the ignored .local directory.
- The tools container's read-only repository bind mount can still read host files;
  its temporary copy excludes .local but not all other secret filename patterns.
  Docker build-context exclusions do not govern bind mounts. Keep real secrets
  outside the repository; this remains a trusted local development tool.
- Docker image tags are mutable and NuGet dependencies are not locked. No package
  vulnerability audit, container runtime test, or comprehensive secret-scanner
  certification is claimed by this review. ZIP archives, ignored local contents,
  and the full Git history were not included in the text scan.
- [Legacy inspection](../CURRENT_STATE.md) already documents credentials in the
  preserved 2022 history. Removal from the current tree does not revoke them;
  owner confirmation of rotation/revocation remains outstanding. No historical
  values were reproduced, tested against services, or changed.

## Validation evidence

Local macOS arm64, SDK 8.0.204, 2026-09-23:

```sh
dotnet restore AegisAI.sln --disable-parallel -m:1 -nr:false
dotnet build AegisAI.sln --configuration Release --no-restore -m:1 -nr:false
dotnet test AegisAI.sln --configuration Release --no-build --no-restore -m:1 -nr:false
```

Restore succeeded; Release build succeeded with zero warnings/errors. All 168
tests passed: 14 Domain, 53 Application, 93 integration/security, 4 architecture,
4 experiments; zero failed/skipped. Local flags serialize MSBuild and disable
node reuse without changing configuration or test selection. The initial sandbox
restore stalled and was stopped; permitted tooling/network access succeeded.

YAML parsing and structural assertions passed for the trigger, permissions, action
pins, credential persistence, full-solution commands, and absence of failure
suppression. Shell syntax, Compose configuration, local Markdown links, and diff
whitespace checks passed. actionlint was unavailable. GitHub-hosted Linux execution
has not occurred; no remote CI success is claimed.

**PLANNED**: first hosted PR run and owner-managed required-check/branch protection,
Actions policy, secret scanning/push protection, and historical credential rotation
verification. Remote settings were neither inspected nor changed. A failed workflow
alone does not establish a merge restriction without repository rules requiring it.
**EXPERIMENTAL**: existing synthetic authorization baselines remain unchanged;
no experiments or research results were produced by this task.
