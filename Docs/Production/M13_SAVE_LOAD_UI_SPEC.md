# M13-04 Save/Load UI — implementation notes (in progress)

The production `VN_Main` modal uses the existing controller, repository, model builder and slot prefab. A dark Direction B panel contains a two-column manual grid, category navigation and an explicit primary action. Slot cards use the M13 shared focus and active indicators, a selected surface, Pretendard and an aspect-preserving thumbnail fitter. The modal retains the existing full-screen CanvasGroup input and thumbnail-suppression contract.

A card click selects its `VNSaveSlotKey`; the primary action re-inspects the current page and delegates to the existing interaction policy and save/load operations. Navigation clears selection. Manual saves retain the existing overwrite confirmation; Auto is read-only in Save mode; Quick uses the existing Quick operation. The mode selector prevents switching to Save when the modal was opened in Load mode, including Title; switching back to Save is available if the modal was originally opened in Save mode. This conservative restriction also applies when Load was opened from gameplay and needs further refinement if gameplay Load-to-Save switching is required.

No save schema, physical slot key, storage path or serialization changes are intended. Corrupt and unsupported data remain protected by the existing operations. The confirmation retains the existing pending slot authority and disables the parent content while active.

This document describes work in progress: production rendering, accessibility, interaction integration, capture, and complete regression must pass before release.
