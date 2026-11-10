# M10-15 — SaveData Compatibility Hardening

**Status: compatibility implementation and validation complete; [PR #64](https://github.com/chocy1849-png/project_all_time/pull/64) open for review.**

M10-15 follows the completed [M10-14 closure](M10_14_PROLOGUE_MILESTONE_CLOSURE_REPORT.md). The production story and line-resume manifest remain unchanged.

## Compatibility contract

The existing exact graph fingerprint remains the fast path. The registry distinguishes it from the LF/CRLF equivalent and rejects unknown identities. New graph identities bind compiled program bytes, the normalized manifest and the base-language line and option text. For the M10 historical identity against a changed graph, the verifier requires every recorded M10 compiled node, variable default, base-language text, and manifest entry to match the pinned descriptor. It rejects runtime-selected node transfers, saliency candidates, and references from old nodes to nodes outside the baseline. This is a deliberately conservative subset; it is not a proof of equivalence for arbitrary Yarn rewrites.

The immutable [M10 baseline descriptor](../../Assets/_Project/Resources/M10_15_BASELINE_DESCRIPTOR.json) was exported from the imported M10 graph after its compiled program SHA-256 and LF/CRLF graph fingerprints matched the historical values. It records 535 compiled node hashes, eight initial variable hashes, 541 line and option text hashes, and all 379 manifest entries. Its canonical LF SHA-256 is `126088f2c52fc7fc5c8eaa860c70240e345e937929578566a7967b59f984a8f0`; runtime validation pins this identity. The exporter refuses to replace existing evidence.

The runtime manifest validator accepts an extensible set of unique line and resume-node mappings, checks compiled line IDs, source nodes and successors, and validates eligibility and restrictions. Original entries retain their existing scene metadata; new entries require chapter and scene labels. `AgentScripts/m10_15_extend_resume_manifest.py` constructs append-only manifests without editing Yarn or the input manifest. The original M10 generator remains the historical prologue-specific tool.

Load and existing-slot replacement use the coordinator's complete pre-mutation validation. Manual confirmation remains in the controller. Quick and Auto slots preserve corrupted, unsupported and invalid snapshots. V2 variable snapshots are checked against declared Yarn types; missing newer variables can fall back to the compiled defaults after the existing clear-and-restore operation. Schema-v1 uses its existing checkpoint path.

## Validation evidence

- A full-source Yarn compilation with isolated additional nodes produced a different compiled program and a 380-entry test manifest. Registry tests accepted an unchanged M10 contract, resolved a non-prologue test line and its scene metadata, and rejected changed historical nodes, successors, command parameters, variable types, restrictions, overlay context and text. These are reconstructed test fixtures, not user saves.
- A focused test exercised the real manual controller with the historical CRLF fingerprint: cancellation preserved the original bytes and confirmation wrote the current fingerprint.
- A variable fixture verified typed old values, rejection of unknown and type-changed entries, and restoration of a newly declared default after repeated loads.
- The focused M5, M8, M9, M10-09, M10-10 and M10-15 regression selection passed **179 / 0 / 0**. The unfiltered EditMode suite passed **524 / 0 / 0** on Unity 6000.3.21f1. An earlier focused run was contaminated by an external Editor status request; the clean rerun passed.
- An isolated Play-mode coordinator/repository fixture restored reconstructed S001-016, S002-009, S005-048, S007-008 and S008-017 saves on both original and expanded graphs, advanced to their original successors, and checked character/background, CG, farewell BGM and information overlay restoration. It also restored a schema-v1 checkpoint, saved and restored a new non-prologue test line, and preserved isolated M8 progress and save bytes. A connected empty-scene Play probe independently completed the S005 restore and successor with frames advancing. These are test fixtures, not historical user saves.
- Manual cancel and confirmation, corrupted and future-schema Quick slots, a full protected Auto ring, load-triggered autosave suppression and old thumbnail removal were exercised in the isolated fixture.
- The generator was checked with an isolated fixture for deterministic 380-entry output, duplicate rejection and byte-for-byte preservation when no entries were added.

## Limitations

The current registry recognizes the M10 epoch only. Saves created against a later graph are loadable on that same graph; a subsequent changed graph requires a separately implemented and reviewed epoch or migration before those saves can be accepted. Descriptor generation for later epochs and arbitrary rewrite migration are not automatic. Text identity covers the base localization; additional localized editions require their own compatibility policy. No commercial release clearance is implied.

## Future narrative update procedure

1. Assign new stable explicit line IDs, chapter and scene labels; preserve all approved historical IDs and descriptor files.
2. Author new source and executable resume nodes with explicit successors and transient restrictions. Keep Replay and technical fixtures outside production save metadata.
3. Use the append-only manifest tool with the approved base, an additions JSON file, the new Yarn source directory and a separate output file. Review the output and compile the complete Yarn project.
4. Run semantic compatibility and historical-save tests against the new compiled graph. A changed baseline node, variable contract, manifest entry or unsupported dynamic control flow requires a reviewed migration; do not replace the old descriptor.
5. Validate actual restoration, successor flow, presentation, audio, slot protection and progress isolation. For a later release, implement and review explicit support for its new immutable epoch before relying on compatibility across another graph change.
