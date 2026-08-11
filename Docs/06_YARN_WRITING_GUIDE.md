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

## M3 presentation commands

- Presentation IDs use stable lowercase snake_case.
- `<<vn_bg background_id>>`
- `<<vn_show character_id expression_id slot_id>>` where slot ID is `far_left`, `left`, `center`, `right`, or `far_right`. Use `default` as an expression ID to select the character's default expression.
- `<<vn_expression character_id expression_id>>`, `<<vn_move character_id slot_id>>`, `<<vn_facing character_id left|right>>`, `<<vn_scale character_id scale>>`, and `<<vn_hide character_id>>` affect only visible characters. M3 has no pose command.
- `<<vn_cg cg_id>>` and `<<vn_clear_cg>>` set and clear the CG Image.
- Commands are immediate in M3; no pose, transition, or fade commands are defined. Animation and Live2D-style systems are out of scope.
- `M3_PRESENTATION_SMOKE` is technical non-canon content. Every visual-state command is followed by a manually advanced checkpoint line; timed waits are not used for M3 verification.
