using System;
using System.Collections.Generic;
using ProjectAllTime.VN.Presentation;
using ProjectAllTime.VN.Records.Achievements;
using ProjectAllTime.VN.Records.Archive;
using ProjectAllTime.VN.Records.Gallery;
using ProjectAllTime.VN.Records.Timeline;
using UnityEngine;
using Yarn.Unity;

namespace ProjectAllTime.VN.Records
{
    /// <summary>
    /// Supplies the existing project authorities needed to validate authored Records definitions.
    /// A null known-ending sequence means ending references are checked intrinsically only.
    /// </summary>
    public sealed class VNRecordsCatalogValidationContext
    {
        private readonly HashSet<string> knownEndingIds;

        public YarnProject YarnProject { get; }
        public VNPresentationCatalog PresentationCatalog { get; }
        public bool HasKnownEndingIds { get; }

        public VNRecordsCatalogValidationContext(
            YarnProject yarnProject,
            VNPresentationCatalog presentationCatalog,
            IEnumerable<string> knownEndingIds = null)
        {
            YarnProject = yarnProject;
            PresentationCatalog = presentationCatalog;
            if (knownEndingIds == null) return;

            HasKnownEndingIds = true;
            this.knownEndingIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in knownEndingIds)
                this.knownEndingIds.Add(id);
        }

        internal bool ContainsKnownEnding(string endingId) => knownEndingIds != null && knownEndingIds.Contains(endingId);
    }

    /// <summary>
    /// Pure, explicit validation of one complete authored Records candidate set against project authorities.
    /// It never changes catalog data or user progress.
    /// </summary>
    public static class VNRecordsCatalogValidator
    {
        public static bool TryValidate(
            VNTimelineCatalog timeline,
            VNCGGalleryCatalog gallery,
            VNArchiveCatalog archive,
            VNAchievementCatalog achievements,
            VNRecordsCatalogValidationContext context,
            out string diagnostic)
        {
            diagnostic = null;
            if (timeline == null || gallery == null || archive == null || achievements == null)
                return Fail("All four Records catalogs are required.", out diagnostic);
            if (context == null)
                return Fail("A Records catalog validation context is required.", out diagnostic);
            if (context.YarnProject == null)
                return Fail("A Yarn Project is required to validate Timeline lines and replay nodes.", out diagnostic);
            if (context.PresentationCatalog == null)
                return Fail("A VN Presentation Catalog is required to validate Gallery CG references.", out diagnostic);

            HashSet<string> yarnNodes;
            HashSet<string> yarnLineIds;
            try
            {
                yarnNodes = new HashSet<string>(context.YarnProject.NodeNames ?? Array.Empty<string>(), StringComparer.Ordinal);
                yarnLineIds = new HashSet<string>(context.YarnProject.GetLineIDsForNodes(yarnNodes), StringComparer.Ordinal);
            }
            catch (Exception exception)
            {
                return Fail($"Yarn Project could not provide its compiled node and line IDs ({exception.GetType().Name}).", out diagnostic);
            }

            if (!TryValidateGallery(gallery, context.PresentationCatalog, out diagnostic)) return false;
            if (!TryValidateArchive(archive, out diagnostic)) return false;
            if (!TryValidateTimeline(timeline, gallery, yarnNodes, yarnLineIds, out diagnostic)) return false;
            return TryValidateAchievements(timeline, gallery, archive, achievements, context, out diagnostic);
        }

        private static bool TryValidateTimeline(
            VNTimelineCatalog catalog,
            VNCGGalleryCatalog gallery,
            HashSet<string> yarnNodes,
            HashSet<string> yarnLineIds,
            out string diagnostic)
        {
            var chapters = new Dictionary<string, VNTimelineChapterDefinition>(StringComparer.Ordinal);
            foreach (var chapter in catalog.Chapters)
            {
                if (chapter == null) return Fail("Timeline catalog contains an empty chapter definition.", out diagnostic);
                if (!VNRecordsCatalogValidation.IsStableId(chapter.ChapterId))
                    return Fail($"Timeline chapter '{chapter.ChapterId}' must use lowercase snake_case.", out diagnostic);
                if (!chapters.TryAdd(chapter.ChapterId, chapter))
                    return Fail($"Timeline catalog contains duplicate chapter ID '{chapter.ChapterId}'.", out diagnostic);
                if (string.IsNullOrWhiteSpace(chapter.DisplayTitle))
                    return Fail($"Timeline chapter '{chapter.ChapterId}' requires a display title.", out diagnostic);
                if (chapter.SortOrder < 0)
                    return Fail($"Timeline chapter '{chapter.ChapterId}' has negative sort order.", out diagnostic);
            }

            var entries = new Dictionary<string, VNTimelineEntryDefinition>(StringComparer.Ordinal);
            foreach (var entry in catalog.Entries)
            {
                if (entry == null) return Fail("Timeline catalog contains an empty entry definition.", out diagnostic);
                if (!VNRecordsCatalogValidation.IsStableId(entry.EntryId))
                    return Fail($"Timeline entry '{entry.EntryId}' must use lowercase snake_case.", out diagnostic);
                if (!entries.TryAdd(entry.EntryId, entry))
                    return Fail($"Timeline catalog contains duplicate entry ID '{entry.EntryId}'.", out diagnostic);
                if (!VNRecordsCatalogValidation.IsStableId(entry.ChapterId) || !chapters.ContainsKey(entry.ChapterId))
                    return Fail($"Timeline entry '{entry.EntryId}' references missing or invalid chapter '{entry.ChapterId}'.", out diagnostic);
                if (string.IsNullOrWhiteSpace(entry.DisplayTitle))
                    return Fail($"Timeline entry '{entry.EntryId}' requires a display title.", out diagnostic);
                if (entry.SortOrder < 0)
                    return Fail($"Timeline entry '{entry.EntryId}' has negative sort order.", out diagnostic);

                if (string.IsNullOrWhiteSpace(entry.DiscoveryLineId))
                    return Fail($"Timeline entry '{entry.EntryId}' requires a discovery Yarn line ID.", out diagnostic);
                if (!yarnLineIds.Contains(entry.DiscoveryLineId))
                    return Fail($"Timeline entry '{entry.EntryId}' references missing Yarn discovery line '{entry.DiscoveryLineId}'.", out diagnostic);
                if (string.IsNullOrWhiteSpace(entry.CompletionLineId))
                    return Fail($"Timeline entry '{entry.EntryId}' requires a completion Yarn line ID.", out diagnostic);
                if (!yarnLineIds.Contains(entry.CompletionLineId))
                    return Fail($"Timeline entry '{entry.EntryId}' references missing Yarn completion line '{entry.CompletionLineId}'.", out diagnostic);

                var milestones = entry.MilestoneLineIds;
                if (milestones == null)
                    return Fail($"Timeline entry '{entry.EntryId}' requires a non-null milestone line list.", out diagnostic);
                var milestoneSet = new HashSet<string>(StringComparer.Ordinal);
                foreach (var lineId in milestones)
                {
                    if (string.IsNullOrWhiteSpace(lineId))
                        return Fail($"Timeline entry '{entry.EntryId}' contains a blank milestone Yarn line ID.", out diagnostic);
                    if (!milestoneSet.Add(lineId))
                        return Fail($"Timeline entry '{entry.EntryId}' contains duplicate milestone line ID '{lineId}'.", out diagnostic);
                    if (!yarnLineIds.Contains(lineId))
                        return Fail($"Timeline entry '{entry.EntryId}' references missing Yarn milestone line '{lineId}'.", out diagnostic);
                }

                if (!milestoneSet.Contains(entry.DiscoveryLineId))
                    return Fail($"Timeline entry '{entry.EntryId}' milestones omit discovery line '{entry.DiscoveryLineId}'.", out diagnostic);
                if (!milestoneSet.Contains(entry.CompletionLineId))
                    return Fail($"Timeline entry '{entry.EntryId}' milestones omit completion line '{entry.CompletionLineId}'.", out diagnostic);

                if (!string.IsNullOrEmpty(entry.ParentEntryId) && !VNRecordsCatalogValidation.IsStableId(entry.ParentEntryId))
                    return Fail($"Timeline entry '{entry.EntryId}' has invalid parent entry ID '{entry.ParentEntryId}'.", out diagnostic);
                if (!string.IsNullOrEmpty(entry.ReplayNode) && !yarnNodes.Contains(entry.ReplayNode))
                    return Fail($"Timeline entry '{entry.EntryId}' references missing replay Yarn node '{entry.ReplayNode}'.", out diagnostic);
                if (!string.IsNullOrEmpty(entry.RelatedCgId) && !VNRecordsCatalogValidation.IsStableId(entry.RelatedCgId))
                    return Fail($"Timeline entry '{entry.EntryId}' has invalid related CG ID '{entry.RelatedCgId}'.", out diagnostic);
            }

            foreach (var entry in catalog.Entries)
            {
                if (!string.IsNullOrEmpty(entry.ParentEntryId))
                {
                    if (!entries.TryGetValue(entry.ParentEntryId, out var parent))
                        return Fail($"Timeline entry '{entry.EntryId}' references missing parent entry '{entry.ParentEntryId}'.", out diagnostic);
                    if (string.Equals(entry.EntryId, entry.ParentEntryId, StringComparison.Ordinal))
                        return Fail($"Timeline entry '{entry.EntryId}' cannot be its own parent.", out diagnostic);
                    if (!string.Equals(entry.ChapterId, parent.ChapterId, StringComparison.Ordinal))
                        return Fail($"Timeline parent '{parent.EntryId}' and child '{entry.EntryId}' must belong to the same chapter.", out diagnostic);
                }

                if (!string.IsNullOrEmpty(entry.RelatedCgId) && !gallery.TryGetCG(entry.RelatedCgId, out _))
                    return Fail($"Timeline entry '{entry.EntryId}' references missing Gallery CG '{entry.RelatedCgId}'.", out diagnostic);
            }

            var visitStates = new Dictionary<string, byte>(StringComparer.Ordinal);
            foreach (var entry in catalog.Entries)
                if (!VisitTimelineParent(entry, entries, visitStates, out diagnostic)) return false;

            diagnostic = null;
            return true;
        }

        private static bool VisitTimelineParent(
            VNTimelineEntryDefinition entry,
            Dictionary<string, VNTimelineEntryDefinition> entries,
            Dictionary<string, byte> visitStates,
            out string diagnostic)
        {
            if (visitStates.TryGetValue(entry.EntryId, out var state))
            {
                if (state == 1) return Fail($"Timeline parent relationships contain a cycle at entry '{entry.EntryId}'.", out diagnostic);
                if (state == 2) { diagnostic = null; return true; }
            }

            visitStates[entry.EntryId] = 1;
            if (!string.IsNullOrEmpty(entry.ParentEntryId) &&
                !VisitTimelineParent(entries[entry.ParentEntryId], entries, visitStates, out diagnostic))
                return false;
            visitStates[entry.EntryId] = 2;
            diagnostic = null;
            return true;
        }

        private static bool TryValidateGallery(
            VNCGGalleryCatalog catalog,
            VNPresentationCatalog presentationCatalog,
            out string diagnostic)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in catalog.Entries)
            {
                if (entry == null) return Fail("Gallery catalog contains an empty entry definition.", out diagnostic);
                if (!VNRecordsCatalogValidation.IsStableId(entry.CgId))
                    return Fail($"Gallery CG '{entry.CgId}' must use lowercase snake_case.", out diagnostic);
                if (!seen.Add(entry.CgId))
                    return Fail($"Gallery catalog contains duplicate CG ID '{entry.CgId}'.", out diagnostic);
                if (string.IsNullOrWhiteSpace(entry.DisplayTitle))
                    return Fail($"Gallery CG '{entry.CgId}' requires a display title.", out diagnostic);
                if (entry.SortOrder < 0)
                    return Fail($"Gallery CG '{entry.CgId}' has negative sort order.", out diagnostic);
                if (!presentationCatalog.TryGetCG(entry.CgId, out var sprite) || sprite == null)
                    return Fail($"Gallery CG '{entry.CgId}' does not resolve to a Sprite through VNPresentationCatalog.", out diagnostic);
            }

            diagnostic = null;
            return true;
        }

        private static bool TryValidateArchive(VNArchiveCatalog catalog, out string diagnostic)
        {
            var categories = new HashSet<string>(StringComparer.Ordinal);
            foreach (var category in catalog.Categories)
            {
                if (category == null) return Fail("Archive catalog contains an empty category definition.", out diagnostic);
                if (!VNRecordsCatalogValidation.IsStableId(category.CategoryId))
                    return Fail($"Archive category '{category.CategoryId}' must use lowercase snake_case.", out diagnostic);
                if (!categories.Add(category.CategoryId))
                    return Fail($"Archive catalog contains duplicate category ID '{category.CategoryId}'.", out diagnostic);
                if (string.IsNullOrWhiteSpace(category.DisplayTitle))
                    return Fail($"Archive category '{category.CategoryId}' requires a display title.", out diagnostic);
                if (category.SortOrder < 0)
                    return Fail($"Archive category '{category.CategoryId}' has negative sort order.", out diagnostic);
            }

            var entries = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in catalog.Entries)
            {
                if (entry == null) return Fail("Archive catalog contains an empty entry definition.", out diagnostic);
                if (!VNRecordsCatalogValidation.IsStableId(entry.ArchiveId))
                    return Fail($"Archive entry '{entry.ArchiveId}' must use lowercase snake_case.", out diagnostic);
                if (!entries.Add(entry.ArchiveId))
                    return Fail($"Archive catalog contains duplicate entry ID '{entry.ArchiveId}'.", out diagnostic);
                if (!VNRecordsCatalogValidation.IsStableId(entry.CategoryId) || !categories.Contains(entry.CategoryId))
                    return Fail($"Archive entry '{entry.ArchiveId}' references missing or invalid category '{entry.CategoryId}'.", out diagnostic);
                if (string.IsNullOrWhiteSpace(entry.DisplayTitle))
                    return Fail($"Archive entry '{entry.ArchiveId}' requires a display title.", out diagnostic);
                if (string.IsNullOrWhiteSpace(entry.Summary))
                    return Fail($"Archive entry '{entry.ArchiveId}' requires a summary.", out diagnostic);
                if (string.IsNullOrWhiteSpace(entry.Body))
                    return Fail($"Archive entry '{entry.ArchiveId}' requires a body.", out diagnostic);
                if (entry.SortOrder < 0)
                    return Fail($"Archive entry '{entry.ArchiveId}' has negative sort order.", out diagnostic);
            }

            diagnostic = null;
            return true;
        }

        private static bool TryValidateAchievements(
            VNTimelineCatalog timeline,
            VNCGGalleryCatalog gallery,
            VNArchiveCatalog archive,
            VNAchievementCatalog catalog,
            VNRecordsCatalogValidationContext context,
            out string diagnostic)
        {
            var achievements = new Dictionary<string, VNAchievementDefinition>(StringComparer.Ordinal);
            foreach (var achievement in catalog.Achievements)
            {
                if (achievement == null) return Fail("Achievement catalog contains an empty definition.", out diagnostic);
                if (!VNRecordsCatalogValidation.IsStableId(achievement.AchievementId))
                    return Fail($"Achievement '{achievement.AchievementId}' must use lowercase snake_case.", out diagnostic);
                if (!achievements.TryAdd(achievement.AchievementId, achievement))
                    return Fail($"Achievement catalog contains duplicate achievement ID '{achievement.AchievementId}'.", out diagnostic);
                if (string.IsNullOrWhiteSpace(achievement.DisplayTitle))
                    return Fail($"Achievement '{achievement.AchievementId}' requires a display title.", out diagnostic);
                if (string.IsNullOrWhiteSpace(achievement.Description))
                    return Fail($"Achievement '{achievement.AchievementId}' requires a description.", out diagnostic);
                if (achievement.SortOrder < 0)
                    return Fail($"Achievement '{achievement.AchievementId}' has negative sort order.", out diagnostic);
                if (achievement.PrerequisiteAchievementIds == null)
                    return Fail($"Achievement '{achievement.AchievementId}' requires a non-null prerequisite list.", out diagnostic);

                if (!TryValidateAchievementCondition(achievement, timeline, gallery, archive, context, out diagnostic))
                    return false;
            }

            foreach (var achievement in catalog.Achievements)
            {
                var seenPrerequisites = new HashSet<string>(StringComparer.Ordinal);
                foreach (var prerequisiteId in achievement.PrerequisiteAchievementIds)
                {
                    if (!VNRecordsCatalogValidation.IsStableId(prerequisiteId))
                        return Fail($"Achievement '{achievement.AchievementId}' has invalid prerequisite ID '{prerequisiteId}'.", out diagnostic);
                    if (!seenPrerequisites.Add(prerequisiteId))
                        return Fail($"Achievement '{achievement.AchievementId}' has duplicate prerequisite '{prerequisiteId}'.", out diagnostic);
                    if (!achievements.ContainsKey(prerequisiteId))
                        return Fail($"Achievement '{achievement.AchievementId}' references missing prerequisite '{prerequisiteId}'.", out diagnostic);
                    if (string.Equals(achievement.AchievementId, prerequisiteId, StringComparison.Ordinal))
                        return Fail($"Achievement '{achievement.AchievementId}' cannot depend on itself.", out diagnostic);
                }
            }

            var visitStates = new Dictionary<string, byte>(StringComparer.Ordinal);
            foreach (var achievement in catalog.Achievements)
                if (!VisitAchievement(achievement, achievements, visitStates, out diagnostic)) return false;

            diagnostic = null;
            return true;
        }

        private static bool TryValidateAchievementCondition(
            VNAchievementDefinition achievement,
            VNTimelineCatalog timeline,
            VNCGGalleryCatalog gallery,
            VNArchiveCatalog archive,
            VNRecordsCatalogValidationContext context,
            out string diagnostic)
        {
            var id = achievement.AchievementId;
            var target = achievement.ConditionTargetId;
            var prerequisites = achievement.PrerequisiteAchievementIds;
            switch (achievement.ConditionType)
            {
                case VNAchievementConditionType.Manual:
                    return RequireEmptyConditionData(achievement, true, out diagnostic);

                case VNAchievementConditionType.EndingCompleted:
                    if (!VNRecordsCatalogValidation.IsStableId(target))
                        return Fail($"Achievement '{id}' EndingCompleted requires a stable ending ID.", out diagnostic);
                    if (context.HasKnownEndingIds && !context.ContainsKnownEnding(target))
                        return Fail($"Achievement '{id}' references unknown ending ID '{target}'.", out diagnostic);
                    return RequireNoThresholdOrPrerequisites(achievement, out diagnostic);

                case VNAchievementConditionType.ChapterUnlocked:
                    if (!VNRecordsCatalogValidation.IsStableId(target) || !timeline.TryGetChapter(target, out _))
                        return Fail($"Achievement '{id}' references missing or invalid Timeline chapter '{target}'.", out diagnostic);
                    return RequireNoThresholdOrPrerequisites(achievement, out diagnostic);

                case VNAchievementConditionType.CGCountAtLeast:
                    if (!RequireEmptyTargetAndPrerequisites(achievement, out diagnostic)) return false;
                    if (achievement.Threshold < 1)
                        return Fail($"Achievement '{id}' CGCountAtLeast threshold must be at least 1.", out diagnostic);
                    if (achievement.Threshold > gallery.Entries.Count)
                        return Fail($"Achievement '{id}' CGCountAtLeast threshold {achievement.Threshold} exceeds Gallery total {gallery.Entries.Count}.", out diagnostic);
                    diagnostic = null;
                    return true;

                case VNAchievementConditionType.ArchiveCountAtLeast:
                    if (!RequireEmptyTargetAndPrerequisites(achievement, out diagnostic)) return false;
                    if (achievement.Threshold < 1)
                        return Fail($"Achievement '{id}' ArchiveCountAtLeast threshold must be at least 1.", out diagnostic);
                    if (achievement.Threshold > archive.Entries.Count)
                        return Fail($"Achievement '{id}' ArchiveCountAtLeast threshold {achievement.Threshold} exceeds Archive total {archive.Entries.Count}.", out diagnostic);
                    diagnostic = null;
                    return true;

                case VNAchievementConditionType.AllCGsUnlocked:
                    if (!RequireEmptyConditionData(achievement, true, out diagnostic)) return false;
                    if (gallery.Entries.Count == 0)
                        return Fail($"Achievement '{id}' cannot use AllCGsUnlocked with an empty Gallery catalog.", out diagnostic);
                    diagnostic = null;
                    return true;

                case VNAchievementConditionType.AllArchiveEntriesUnlocked:
                    if (!RequireEmptyConditionData(achievement, true, out diagnostic)) return false;
                    if (archive.Entries.Count == 0)
                        return Fail($"Achievement '{id}' cannot use AllArchiveEntriesUnlocked with an empty Archive catalog.", out diagnostic);
                    diagnostic = null;
                    return true;

                case VNAchievementConditionType.AllOfAchievements:
                    if (!IsEmpty(target))
                        return Fail($"Achievement '{id}' AllOfAchievements must have an empty condition target.", out diagnostic);
                    if (achievement.Threshold != 0)
                        return Fail($"Achievement '{id}' AllOfAchievements must have threshold 0.", out diagnostic);
                    if (prerequisites.Count == 0)
                        return Fail($"Achievement '{id}' AllOfAchievements requires at least one prerequisite.", out diagnostic);
                    diagnostic = null;
                    return true;

                default:
                    return Fail($"Achievement '{id}' has unknown condition type value '{achievement.ConditionType}'.", out diagnostic);
            }
        }

        private static bool RequireEmptyConditionData(VNAchievementDefinition achievement, bool includeTarget, out string diagnostic)
        {
            if (includeTarget && !IsEmpty(achievement.ConditionTargetId))
                return Fail($"Achievement '{achievement.AchievementId}' {achievement.ConditionType} must have an empty condition target.", out diagnostic);
            if (achievement.Threshold != 0)
                return Fail($"Achievement '{achievement.AchievementId}' {achievement.ConditionType} must have threshold 0.", out diagnostic);
            if (achievement.PrerequisiteAchievementIds.Count != 0)
                return Fail($"Achievement '{achievement.AchievementId}' {achievement.ConditionType} must have no prerequisites.", out diagnostic);
            diagnostic = null;
            return true;
        }

        private static bool RequireNoThresholdOrPrerequisites(VNAchievementDefinition achievement, out string diagnostic)
        {
            if (achievement.Threshold != 0)
                return Fail($"Achievement '{achievement.AchievementId}' {achievement.ConditionType} must have threshold 0.", out diagnostic);
            if (achievement.PrerequisiteAchievementIds.Count != 0)
                return Fail($"Achievement '{achievement.AchievementId}' {achievement.ConditionType} must have no prerequisites.", out diagnostic);
            diagnostic = null;
            return true;
        }

        private static bool RequireEmptyTargetAndPrerequisites(VNAchievementDefinition achievement, out string diagnostic)
        {
            if (!IsEmpty(achievement.ConditionTargetId))
                return Fail($"Achievement '{achievement.AchievementId}' {achievement.ConditionType} must have an empty condition target.", out diagnostic);
            if (achievement.PrerequisiteAchievementIds.Count != 0)
                return Fail($"Achievement '{achievement.AchievementId}' {achievement.ConditionType} must have no prerequisites.", out diagnostic);
            diagnostic = null;
            return true;
        }

        private static bool VisitAchievement(
            VNAchievementDefinition achievement,
            Dictionary<string, VNAchievementDefinition> achievements,
            Dictionary<string, byte> visitStates,
            out string diagnostic)
        {
            if (visitStates.TryGetValue(achievement.AchievementId, out var state))
            {
                if (state == 1) return Fail($"Achievement dependency graph contains a cycle at '{achievement.AchievementId}'.", out diagnostic);
                if (state == 2) { diagnostic = null; return true; }
            }

            visitStates[achievement.AchievementId] = 1;
            foreach (var prerequisiteId in achievement.PrerequisiteAchievementIds)
                if (!VisitAchievement(achievements[prerequisiteId], achievements, visitStates, out diagnostic)) return false;
            visitStates[achievement.AchievementId] = 2;
            diagnostic = null;
            return true;
        }

        private static bool IsEmpty(string value) => string.IsNullOrEmpty(value);

        private static bool Fail(string message, out string diagnostic)
        {
            diagnostic = message;
            return false;
        }
    }

    /// <summary>Shared lowercase ASCII snake_case rule for authored Records IDs.</summary>
    public static class VNRecordsCatalogValidation
    {
        public static bool IsStableId(string value)
        {
            if (string.IsNullOrEmpty(value) || value[0] < 'a' || value[0] > 'z') return false;

            var previousWasSeparator = false;
            for (var index = 1; index < value.Length; index++)
            {
                var character = value[index];
                var isLowerLetter = character >= 'a' && character <= 'z';
                var isDigit = character >= '0' && character <= '9';
                if (isLowerLetter || isDigit)
                {
                    previousWasSeparator = false;
                    continue;
                }

                if (character != '_' || previousWasSeparator || index == value.Length - 1)
                    return false;
                previousWasSeparator = true;
            }

            return true;
        }
    }
}
