# Security Policy

## Supported Versions

The latest published NuGet release receives security updates. Older versions are not actively patched — upgrade to the latest minor release first when reporting an issue.

| Version | Supported |
| ------- | --------- |
| Latest minor | ✅ |
| Older | ❌ |

## Reporting a Vulnerability

**Please do not open a public GitHub issue for security problems.**

Use GitHub's [private vulnerability reporting](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/security/advisories/new) (preferred), or email <hello@danielapoliveira.com>.

When reporting, include:

- A clear description of the issue and the impact you observed.
- A minimal reproduction (LINQ snippet, schema, query, or repro repo).
- Affected version(s) of `Equibles.ParadeDB.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, .NET runtime, and ParadeDB / pg_search.
- Any suggested remediation, if applicable.

### Response timeline

- **Acknowledgement**: within 3 business days.
- **Initial assessment**: within 7 business days.
- **Fix or mitigation**: depends on severity; coordinated disclosure expected before any public details.

Reporters are credited in the release notes unless they prefer to remain anonymous.
