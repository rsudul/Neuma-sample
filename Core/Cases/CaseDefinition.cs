using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neuma.Core.Cases
{
    /// <summary>
    /// Validated, immutable case definition used by the domain.
    /// </summary>
    public sealed class CaseDefinition
    {
        public string CaseId { get; }
        public string EntryDialogueId { get; }
        public IReadOnlyList<string> DialogueIds { get; }
        public IReadOnlyDictionary<string, string> DialogueToTranscript { get; }
        public IReadOnlyDictionary<string, string> DialogueToSubject { get; }

        public CaseDefinition(string caseId, string entryDialogueId, IEnumerable<CaseDialogueData> dialogues)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(entryDialogueId))
            {
                throw new ArgumentException("EntryDialogueId cannot be null or whitespace.", nameof(entryDialogueId));
            }

            if (dialogues == null)
            {
                throw new ArgumentNullException(nameof(dialogues));
            }

            CaseId = caseId;
            EntryDialogueId = entryDialogueId;

            var dialogueIds = new List<string>();
            var transcriptMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var subjectMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dialogue in dialogues)
            {
                if (dialogue == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(dialogue.DialogueId))
                {
                    continue;
                }

                dialogueIds.Add(dialogue.DialogueId);

                if (!string.IsNullOrWhiteSpace(dialogue.TranscriptId))
                {
                    transcriptMap[dialogue.DialogueId] = dialogue.TranscriptId;
                }

                if (!string.IsNullOrWhiteSpace(dialogue.SubjectId))
                {
                    subjectMap[dialogue.DialogueId] = dialogue.SubjectId;
                }
            }

            if (dialogueIds.Count == 0)
            {
                throw new ArgumentException("Case must define at least one dialogue.", nameof(dialogues));
            }

            if (!transcriptMap.ContainsKey(entryDialogueId))
            {
                throw new ArgumentException(
                    "Transcript map must contain an entry for the entry dialogue id.", nameof(dialogues));
            }

            DialogueIds = new ReadOnlyCollection<string>(dialogueIds);
            DialogueToTranscript = new ReadOnlyDictionary<string, string>(transcriptMap);
            DialogueToSubject = new ReadOnlyDictionary<string, string>(subjectMap);
        }
    }
}

