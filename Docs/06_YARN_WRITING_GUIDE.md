# M1 Yarn Writing Guide

This guide records only the M1 technical rules established with Yarn Spinner 3.2.7.

- Yarn files are plain `.yarn` text assets.
- All variables must be explicitly declared.
- Node IDs use stable uppercase snake case.
- Production nodes should use a section prefix, for example `PROLOGUE_001_START`.
- Technical test nodes use the `M1_` prefix.
- Variable names use lowercase snake case.
- Boolean story flags use `$flag_...`.
- Numeric relationship values may use `$rel_...`.
- Counters may use `$count_...`.
- Node IDs should be treated as persistent identifiers once Save/Load is introduced.
- M1 does not define production custom commands.
- M1 does not define persistence behavior.
