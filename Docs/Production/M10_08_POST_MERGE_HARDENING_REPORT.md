# M10-08 Post-Merge Hardening Report

**M10-08 POST-MERGE TECHNICAL HARDENING READY FOR REVIEW**

**PRODUCTION MEDIA ACCEPTANCE PENDING**

## Scope and baseline

- Repository: project_all_time
- Baseline commit: 595f12a024f7cfc87ec0fcb0eac2851e8b94c9a4
- Corrective branch: fix/m10-08-post-merge-hardening
- Unity Editor: 6000.3.21f1; Unity CLI: 1.0.0-beta.15
- The canonical source SHA-256 remains 8413E847DD66D7D45585847A935B9C384D91FACFE9724EE27084F3D86FF977E9. The source file and M5 save schema were not changed.
- The three production Yarn files still contain 541 explicit line IDs. The frozen 379 production paragraphs and 103 presentation beats remain covered by the existing source and beat checks. No canonical dialogue was edited.
- The original M10_08_FINAL_MEDIA_INTEGRATION_REPORT.md and its 36 direct-copy source/import hash records were preserved. This corrective change does not modify or reimport the media collection.

## Findings and corrections

### S008 audio timeline

The composite sfx_connection_rain_handoff is a finite 4.250-second source WAV (4.250667 seconds in the Unity imported clip). Its equipment-to-rain crossfade starts at 1.75 seconds and completes when the 2.760-second equipment source ends.

| Event | Before correction, relative to handoff onset | Corrected source timing |
| --- | ---: | ---: |
| Equipment connection starts | 0.000 s | 0.000 s |
| Rain enters the composite | 1.750 s | 1.750 s |
| Equipment-to-rain crossfade completes | 2.760 s | 2.760 s |
| Discrete awning-drop command | 1.000 s | 3.000 s |
| Bus-brake command | 2.000 s | after sfx_wait observes the handoff and drop clip end |
| P13 narration | 3.000 s | one second after bus onset |
| P14 title | immediately after the P13 line | after sfx_wait observes the bus clip end |

Previously, the drop command preceded rain onset by 0.75 seconds, and the bus started about 2.25 seconds before the handoff ended. The 13.378-second bus clip then had no completion barrier before the title. From the source timeline, P13 could begin with roughly 12.38 seconds of bus audio remaining and the title was not gated on the tail. This is a source-logic finding; the pre-correction title/bus overlap was not independently recorded in a live Play probe.

The corrected S008-P12 Yarn waits three seconds before issuing the distinct drop cue, then waits on the controller's current finite one-shot deadline before issuing the bus cue. The barrier reads the imported clip's sample count, frequency, and AudioSource pitch, and also waits until the live SFX AudioSource reports no playback. Unity documents that AudioSource.isPlaying includes PlayOneShot playback ([Unity 6.0 API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource-isPlaying.html)). It does not use a duration copied from the silent placeholder. The existing one-shot AudioSource remains the only owner; no Ambient subsystem or looping SFX was added. Each of the three S008 cues appears once.

P13 narration is allowed to accompany the bus. A second sfx_wait after that authored line gates P14 until the bus tail ends. The full-route Play run recorded the handoff-to-bus onset interval as 4.267 seconds and P13 line start at bus +1.003 seconds; in that timestamp-only probe, the title appeared 18 ms after the duration-derived deadline. The final staged P12 probe with the source-state guard independently observed P13 displayed while the source was playing and the wait pending, then the title at DSP 1078.101 against deadline 1078.083 with both source playback and pending state false.

DialogueRunner.Stop() cancels Yarn progression but does not stop an already-issued SFX one-shot; its tail continues through the existing AudioSource. Load normalization stops that source and resets the pending deadline, so stale SFX do not leak into a restored state. AudioRestore_NormalizesToCanonicalSourceAndStopsStaleSfx also verifies that a live muted PlayOneShot keeps the wait pending after its estimated deadline, and that Load releases the wait.

The isolated drop remains inaudible because sfx_raindrop_awning still resolves to the silent fallback. The ordering and wait logic are verified; the audible drop itself is not.

### Six-slot state and Replay compatibility

Review found no concrete compatibility defect requiring removal of VNCharacterSlot.OuterFarRight / serialized outer_far_right. Production and Replay retain their own serialized slot views and rendering authorities.

ProductionMorningSixCharacterSnapshot_RoundTripsAndLegacyFiveSlotStateRemainsCompatible builds the authored morning family state with minseok, jinhee, jihun, rope_memory, junho, and narae in six unique slots. It verifies distinct body sprites, expression resolution, facing and scale, capture, read-only restore preparation, state clearing, restoration, and reconstructed assignments. Existing invalid/duplicate logical-state validation remains in place. The same test restores a valid legacy five-character snapshot. No M5 schema field was added.

The isolated Replay integration fixture now also captures, clears, prepares, and restores all six family characters, checking the six slot assignments, facing, scale, visibility, and subsequent Yarn cleanup. VNMainM9RecordsWiringAuditTests and VNReplayRuntimeIsolationTests continue to verify separate Replay ownership and references. This is controller-level compatibility only; it is not the M10-09 production checkpoint-to-file round trip.

### Expression invariant

The expression test no longer requires every headSprite to remain null. It still requires exactly 19 character-scoped keys, unique identities, and no global expression IDs. Optional supplied head art is permitted; when absent, the body image remains visible and the head layer safely remains hidden. A separate delivery gap remains for expressive art; no PNG was fabricated.

## Temporary resources and remaining media

A fresh source-folder scan found 26 PNGs and 16 WAVs in the supplied folders. No suitable final replacement was present for the six frozen IDs below. The catalog and importer regression tests confirm that these IDs still resolve safely.

| Frozen ID | Intended scene / cue | Current catalog resource | Required final delivery |
| --- | --- | --- | --- |
| rope | S006 reveal after canonical L924, then S007–S008 | Assets/_Project/Art/TemporaryM10/TEMPORARY_cyber_dog_marker.png | Distinct mechanical Rope character art; preserve identity separation from rope_memory. |
| sfx_plate_counter | S001-P01 | Assets/_Project/Audio/TEMPORARY_M10_Silence.wav | sfx_plate_counter.wav, a suitable plate/counter cue. No suitable source file found. |
| sfx_child_entry_steps | S001-P06 | Assets/_Project/Audio/TEMPORARY_M10_Silence.wav | sfx_child_entry_steps.wav, child entry footsteps. No suitable source file found. |
| sfx_sink_washing | S005-P14 | Assets/_Project/Audio/TEMPORARY_M10_Silence.wav | sfx_sink_washing.wav, sink/washing sound. No suitable source file found. |
| sfx_slipper_toe_tap | S006-P01, issued twice by the authored node | Assets/_Project/Audio/TEMPORARY_M10_Silence.wav | sfx_slipper_toe_tap.wav, a distinct slipper toe tap. No suitable source file found. |
| sfx_raindrop_awning | S008-P12 | Assets/_Project/Audio/TEMPORARY_M10_Silence.wav | Isolated awning drop. A supplied sfx_raindrop_awning.wav exists, but it is 12.714 seconds and correlates 0.9999955 with the rain-splash recording; it is not an isolated drop and was not substituted. |

## Expression-art delivery register

All 19 scoped keys currently have no separate headSprite. Existing body art remains the compatible fallback. Any delivered head art must preserve alpha, align with the current head layer and eye line, and match the existing body scale. It must either be supplied as an independent Head Sprite referenced by VNCharacterDefinition.expressions[].headSprite, or as revised layered source art that can be integrated through that field.

| Character / key | Intended scene or beat | Existing default body | Missing content / delivery |
| --- | --- | --- | --- |
| jihun/default | S001–S008 | Assets/_Project/Art/Characters/jihun/jihun_default.png | Default body is present; no separate default head is required for fallback. |
| jihun/uneasy | S002, S003, S006, S007 | Assets/_Project/Art/Characters/jihun/jihun_default.png | Unsettled eyes/brows; aligned head layer. |
| jinhee/default | S001–S006 | Assets/_Project/Art/Characters/jinhee/jinhee_default.png | Default body is present; no separate default head is required for fallback. |
| jinhee/smile | S001, S002, S005 | Assets/_Project/Art/Characters/jinhee/jinhee_default.png | Warm smile that preserves Jinhee's existing identity. |
| jinhee/uneasy | S001–S004 | Assets/_Project/Art/Characters/jinhee/jinhee_default.png | Dawning unease in eyes/brows. |
| jinhee/crying | S004–S006 | Assets/_Project/Art/Characters/jinhee/jinhee_default.png | Restrained tears; priority 1. |
| jinhee/firm | S004–S006 | Assets/_Project/Art/Characters/jinhee/jinhee_default.png | Resolve with tears retained; priority 1. |
| minseok/default | S001–S005 | Assets/_Project/Art/Characters/minseok/minseok_default.png | Default body is present; no separate default head is required for fallback. |
| minseok/quiet | S001–S005 | Assets/_Project/Art/Characters/minseok/minseok_default.png | Restrained, attentive expression. |
| narae/default | S001–S005 | Assets/_Project/Art/Characters/narae/narae_default.png | Default body is present; no separate default head is required for fallback. |
| junho/default | S001–S005 | Assets/_Project/Art/Characters/junho/junho_default.png | Default body is present; no separate default head is required for fallback. |
| junho/pout | S001–S005 | Assets/_Project/Art/Characters/junho/junho_default.png | Small breakfast pout. |
| junho/eager | S001–S005 | Assets/_Project/Art/Characters/junho/junho_default.png | Shoe/arrival anticipation. |
| rope_memory/default | S001–S006, before awakening | Assets/_Project/Art/Characters/rope_memory/rope_memory_default.png | Ordinary Rope body is present; keep visually distinct from mechanical rope. |
| rope_memory/alert | S002, S003, S006 | Assets/_Project/Art/Characters/rope_memory/rope_memory_default.png | Attentive head while retaining ordinary Rope identity. |
| rope/default | S006 after L924, S007–S008 | Assets/_Project/Art/TemporaryM10/TEMPORARY_cyber_dog_marker.png | Mechanical Rope body art and matching default head are still missing. |
| rope/alert | S006 after L924, S007–S008 | Assets/_Project/Art/TemporaryM10/TEMPORARY_cyber_dog_marker.png | Mechanical Rope body art plus an expressive attentive head are still missing. |
| dokyung/default | S007 | Assets/_Project/Art/Characters/dokyung/dokyung_default.png | Default body is present; no separate default head is required for fallback. |
| eunjung/default | S008 | Assets/_Project/Art/Characters/eunjung/eunjung_default.png | Default body is present; no separate default head is required for fallback. |

## Audio inspection

The following are non-destructive measurements of the source/derived PCM WAV data. RMS is dBFS; an integrated LUFS/R128 measurement was not available. Values are not claims about perceived loudness after mixer routing or dialogue playback.

| Asset | Duration / format | Peak | RMS | First / last second RMS | Other observations |
| --- | --- | ---: | ---: | ---: | --- |
| bgm_ordinary_morning | 96.000 s, stereo 48 kHz PCM16 | −4.93 dBFS | −18.84 dBFS | −17.64 / −42.03 dBFS | No clipped samples or silence at edges. |
| bgm_quiet_farewell | 193.320 s, stereo 48 kHz PCM16 | −3.50 dBFS | −20.97 dBFS | −19.26 / −56.73 dBFS | 0.131 s trailing below −60 dBFS; no clipped samples. |
| sfx_low_vibration | 6.615 s, stereo 48 kHz PCM16 | −3.26 dBFS | −17.89 dBFS | −25.01 / −40.29 dBFS | No clipped samples. |
| sfx_connection_rain_handoff | 4.250 s nominal, stereo 48 kHz PCM16 | −9.84 dBFS | −27.63 dBFS | −30.43 / −34.13 dBFS | 0.007 s leading and 0.009 s trailing silence; no clipping. |
| sfx_old_bus_brake | 13.378 s nominal, stereo 44.1 kHz PCM16 | −0.09 dBFS | −19.87 dBFS | −30.53 / −34.34 dBFS | 0.067 s leading and 0.150 s trailing near-silence; no clipped samples. |

The source WAVs are stereo and retain their authored channel layout. Unity imports both BGM clips as Streaming/Vorbis quality 0.9 with looping and the existing 0.25 catalog gain; SFX use DecompressOnLoad with stereo preserved. At equal source measurement conditions, the morning BGM is about 2.13 dB higher RMS than the farewell BGM. Both remain at the existing 0.25 gain. The morning steady-low adaptation remains provisional; no persistent story-gain system or arbitrary normalization was added. The bus peak is close to full scale but not clipped; its existing 0.18 catalog gain and the user's M7 mixer preferences were not changed.

The Mixkit-labeled rain component is a 15-second stereo 44.1 kHz PCM16 sound effect, not stock music; its source has four full-scale samples. It was resampled and incorporated into the derived finite composite; the resulting composite has no clipping. Processing and hash are unchanged from the original integration report:

- Sources: sfx_equipment_disconnect_alt.wav (source SHA-256 CF9BBE5C402F9E9941371CC2C06864EDF11F71C01FABC746EE6E9AE399E79109) and rain/mixkit-light-rain-loop-2393.wav (source SHA-256 0E90FA5F678CCA6966E66477B75B8FE608BF845D54E4B3E3EFFF839B9CC6805E).
- Processing: equipment at 0.40 gain; a 2.5-second rain excerpt beginning at 8.4 seconds at 0.80 gain; rain resampled from 44.1 to 48 kHz; equal-power crossfade begins at 1.75 seconds; 10 ms edge fade-in and 0.5-second finite tail fade; no normalization or limiter.
- Derived Assets/_Project/Audio/SFX/sfx_connection_rain_handoff.wav SHA-256: 6B490645878EBEC335686A334A869382F60ED58EF562108059E0699A70D6BD52.

No human acoustic audition was performed. Source-logic timing and imported clip durations are verified; perceived onset, balance against dialogue, tone, fatigue, and final sound acceptance remain pending. No gain changes were made.

## Provenance and license status

- **User-provided image and BGM candidates:** the original M10-08 report records the source and imported SHA-256 for all direct copies and labels them user-supplied/creator-claimed. Those hashes remain untouched. A named creator/generator, per-file export history, third-party source declarations, and commercial-use release evidence are absent from the supplied assets, so these remain candidate media rather than rights-cleared final media.
- **Mixkit-labeled rain:** the official [Mixkit rain sound-effects listing](https://mixkit.co/free-sound-effects/rain/) currently identifies a 15-second “Light rain loop” under sound effects. The [Mixkit Sound Effects Free License](https://mixkit.co/license/modal/sfxFree/) permits incorporation into video games and other larger end products, and restricts standalone redistribution, bundled source-file redistribution, claiming the item as one's own, and rights-management registration. The direct download receipt or stable exact-item page tying this local WAV to that listing and license class was not retained. The local filename and category match are not proof of origin; source-specific rights acceptance remains pending.
- **Korean font:** Assets/_Project/Fonts/NotoSansKR/NotoSansKR-Regular.ttf, SHA-256 7715af52f5fe77153ce5678546258993982d2da61abea8d25fb89eb5aaec5ca6. The font name table reports Noto Sans CJK KR v2.004 and Adobe copyright 2014–2021; the PostScript name is NotoSansCJKkr-Thin, which does not match the file/import name “Regular”. The official [Noto CJK Sans license](https://github.com/notofonts/noto-cjk/blob/main/Sans/LICENSE) is SIL OFL 1.1; the official [release notes](https://github.com/notofonts/noto-cjk/blob/main/Sans/NEWS.md) identify v2.004, and the [Noto use guide](https://github.com/notofonts/noto-docs/blob/main/docs/website/use.md) explains app bundling. OFL redistribution requires retaining the license/copyright notice and observing reserved-name conditions; the font cannot be sold by itself. Because the exact file/style identity is not reconciled, project-level provenance remains UNVERIFIED and it is not added to a final release manifest.

## Visual review

- Existing evidence in Docs/Production/Evidence/M10_08/ was reviewed. family_table_morning.png, family_storage_doorway.png, and family_table_sunset.png show five human cutouts plus ordinary rope_memory, with no duplicated people painted into the background plates. Proportions and alpha cutouts are coherent across the scenes. Rope remains beside Jihun at the existing small scale and offset. Faces and text stay readable; the dialogue panel covers portions of lower bodies as in the existing UI composition.
- In the live VN_Main run, all six family figures were visible in the morning staging. The medical diagram appeared inside its panel without covering faces or dialogue. The mechanical Rope marker is a visible temporary white tile in the later scene; it is an unresolved asset, not a cursor or accepted design.
- The existing cyber_dog_reveal_temporary_marker.png documents that placeholder and does not establish final art acceptance.
- cg_present_table_glimpse remains an unoccupied, wide dining-room image with a camera different from the intended same-camera family-table framing. It is shown briefly as a CG insert. A safe crop cannot correct the camera/composition mismatch; revised source composition is required. No empty-chair cue was added.
- The final title capture showed “1부 — 깨우는 사람 / 끝” centered and readable on black. This is technical presentation validation, not user art-direction sign-off.

## Validation

### Focused EditMode runs

| Filter / coverage group | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| M10_08ProductionMediaTests — frozen catalog IDs, import settings, expression fallback, S008 timing, composition | 8 | 0 | 0 |
| VNFullSnapshotTests — six-slot capture/restore, legacy five-slot state, load/audio normalization | 10 | 0 | 0 |
| VNReplayIntegrationTests — isolated Replay, six-slot restore, Yarn cleanup | 4 | 0 | 0 |
| ProductionFont_CoversNewYarn | 1 | 0 | 0 |
| VNMainM6WiringAuditTests | 1 | 0 | 0 |
| VNMainM7SettingsWiringAuditTests | 7 | 0 | 0 |
| VNMetaProgress group | 61 | 0 | 0 |
| VNPrologue group — S004 hold, localized effects, profile flow, source/font checks | 19 | 0 | 0 |
| VNLocalizedEffectTests | 2 | 0 | 0 |
| VNMainM9RecordsWiringAuditTests | 7 | 0 | 0 |
| VNReplayRuntimeIsolationTests | 2 | 0 | 0 |
| VNRecords group | 103 | 0 | 0 |

The filters overlap; these focused totals are not summed as a unique test count.

### Full EditMode run

Unity 6000.3.21f1, unfiltered EditMode: **495 passed / 0 failed / 0 skipped / 0 inconclusive**. The two tests added by this hardening account for the increase over the historical 493-test baseline. Script compilation completed with no errors.

### Unity Play validation

- Scene: actual VN_Main; controlled entry: PROLOGUE_S001_START. The serialized technical Start Node remains M2_UI_START.
- The controlled S001–S008 run reached the P14 title and stopped normally. Six-character family staging, ordinary versus mechanical Rope identity, the S006 localized white-door effect, S007 diagnosis diagram, and S008 profile continuation were observed. The existing profile Button handlers opened Sunju, Haejin, and Hana in order; each was reviewed, Back was used, and Continue was clicked only when enabled.
- At S004-P07 line:prologue_s004_034, the manual-hold flag was true. VNLineAdvancerInputBridge.TryAdvance(Auto) returned false while TryAdvance(Manual) returned true; the line resumed only after manual input.
- At S007-P05 line:prologue_s007_028, M1007MedicalDiagram was active at full alpha. The final profile state had all three reviews complete before Continue. The instrumented run observed 384 read IDs and the finale title.
- S008 Play timing: the full-route run recorded handoff-to-bus at 4.267 s and P13 line start at bus +1.003 s. In a final staged P12 probe with the source-state guard, the fully displayed P13 line still had the bus source playing and the wait pending; the title became visible 0.018 s after the imported bus end deadline, with source playback and pending state both false. The drop is still silent, so this confirms source scheduling and one-shot timing authority, not the sound of a raindrop.
- The Play-console delta contained no new errors or warnings; compilationFailed=false and Editor ground truth was 0 errors. Test recompilation emitted the existing Unity AI editor-only reference and nullable-context warnings; no compiler errors occurred.

No actual production checkpoint file was saved and loaded. No M10-13 user Play Gate or full M10 completion is claimed.

## Documentation hygiene and M10-09 handoff

M10_08_FINAL_MEDIA_INTEGRATION_REPORT.md remains the source of the complete import and direct-copy hash ledger; its existing local source paths were not rewritten. This new report uses canonical project-relative paths. Docs/Production/M10_TEMPORARY_ASSET_MAPPING.md starts with a Korean BGM review rubric instead of an asset mapping; it appears to contain unrelated user-authored content, so it was left unchanged and is reported separately.

M10-09 should handle the real production checkpoint-to-file round trip, re-entry/transient media checks, delivery and integration of the six temporary resources, the 19-key expression register (prioritizing Jinhee's crying and firm states), exact source/license receipts, Korean font identity reconciliation, human acoustic audition, and user art/audio acceptance. M10-09 was not started in this change.
