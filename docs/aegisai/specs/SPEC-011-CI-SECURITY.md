# SPEC-011 — GitHub CI and repository security baseline

Date: 2026-09-23  
Task: AEGISAI-011  
Status: **IMPLEMENTED** in working tree; pending review, not committed.

## Workflow contract

[CI](../../../.github/workflows/ci.yml) runs on pushes to main and pull requests targeting main,
including drafts, without path filters. The default pull_request activities cover
opening, reopening, and updating a PR. Checkout uses the PR merge revision or the
pushed commit for push events. One GitHub-hosted ubuntu-24.04 job, named Build and test, runs:

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

Per the final task's preference, trusted official actions use major-version pins:
actions/checkout@v6 and actions/setup-dotnet@v5. No third-party actions are used.
These replace the initial full-commit pins; major tags can move as upstream publishes
updates, so they do not provide immutable action provenance. Major upgrades require
review. The SDK and hosted runner also receive updates, so this
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
  vulnerability audit or comprehensive secret-scanner certification is claimed.
  The final text scan covered 258 tracked text files and ZIP archive members,
  with zero candidate matches. Ignored local contents, untracked experiment
  artifacts, and the full Git history were not included in that scan.
- [Legacy inspection](../CURRENT_STATE.md) already documents credentials in the
  preserved 2022 history. Removal from the current tree does not revoke them;
  owner confirmation of rotation/revocation remains outstanding. No historical
  values were reproduced, tested against services, or changed.

## Validation evidence

Final validation: local macOS arm64, SDK 8.0.204, 2026-09-23:

```sh
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

Restore succeeded; Release build succeeded with zero warnings/errors. All 168
tests passed: 14 Domain, 53 Application, 93 integration/security, 4 architecture,
4 experiments; zero failed/skipped. These exact commands ran with permitted tooling
and network access. Earlier serialized validation remains recorded in the log.
Passing invalid-request integration tests emitted DeveloperExceptionPage error
events for BadHttpRequestException; these are exercised rejection paths, not failed
tests. Local Data Protection informational logs also describe unencrypted local
profile key storage; no production key-storage guarantee is inferred.

YAML parsing and structural assertions passed for the trigger, permissions, action
pins, credential persistence, full-solution commands, and absence of failure
suppression. Shell syntax, Compose configuration, local Markdown links, and diff
whitespace checks passed. Ignore checks confirmed .env, .env.*, .idea/, .DS_Store,
bin/, and obj/ are excluded at root and nested paths, while .env.example is allowed.
actionlint was unavailable. The final workflow has not been run on GitHub by this
task; no remote CI success is claimed.

## Docker evidence and V0.1 limitations

The user reported successful manual Docker startup, ASP.NET Core listening on
port 8080, and GET /health returning HTTP 200. During final verification,
./dev health also exited successfully and returned {"status":"healthy"}.
The tools container emitted: "An issue was encountered verifying workloads."
This warning did not prevent the health check; no workload update was performed.
The optional demo and ci-validation experiment were not rerun in this final task.

The user also reported ASP.NET Core Data Protection warnings about ephemeral/
in-memory key storage. The current development environment has no configured
durable Data Protection key store; protected payloads relying on those keys may
become unreadable across restarts. Production key persistence, access protection,
and sharing require separate design and validation. These keys are distinct from
the development JWT signing key, which is intentionally discarded after issuance.
Successful /health demonstrates liveness only. Docker V0.1 is an experimental
development environment, not a production-ready deployment.

## Research integrity

Models A/B/C and tested supporting functionality are **IMPLEMENTED**. Behavioral/
AI Model D remains **PLANNED**. The runner and synthetic smoke artifacts are
**EXPERIMENTAL** and do not establish superiority, independent adoption, or
production readiness. Final documentation review found no affirmative unsupported
claims of those capabilities; stale statements denying the existing smoke
artifacts were corrected in current research/architecture summaries.

**PLANNED**: first hosted PR run and owner-managed required-check/branch protection,
Actions policy, secret scanning/push protection, and historical credential rotation
verification. Remote settings were neither inspected nor changed. A failed workflow
alone does not establish a merge restriction without repository rules requiring it.
**EXPERIMENTAL**: existing synthetic authorization baselines remain unchanged;
no experiments or research results were produced by this task.
