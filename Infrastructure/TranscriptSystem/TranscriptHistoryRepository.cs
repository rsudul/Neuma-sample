using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neuma.Core.TranscriptSystem;

namespace Neuma.Infrastructure.TranscriptSystem
{
    /// <summary>
    /// In-memory implementation of ITranscriptHistoryRepository.
    /// Stores runtime transcript history for the current game session.
    /// </summary>
    public sealed class TranscriptHistoryRepository : ITranscriptHistoryRepository
    {
        private readonly Dictionary<string, Dictionary<string, List<TranscriptLine>>> _linesByCaseAndTranscript =
            new Dictionary<string, Dictionary<string, List<TranscriptLine>>>(StringComparer.OrdinalIgnoreCase);

        private readonly object _syncRoot = new object();

        public IReadOnlyList<TranscriptLine> GetAllLines(string caseId, string transcriptId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(transcriptId))
            {
                throw new ArgumentException("TranscriptId cannot be null or whitespace.", nameof(transcriptId));
            }

            lock (_syncRoot)
            {
                if (_linesByCaseAndTranscript.TryGetValue(caseId, out var byTranscript) &&
                    byTranscript.TryGetValue(transcriptId, out var list))
                {
                    return new ReadOnlyCollection<TranscriptLine>(list);
                }

                return Array.Empty<TranscriptLine>();
            }
        }

        public IReadOnlyList<TranscriptLine> GetAllLinesForCase(string caseId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            lock (_syncRoot)
            {
                if (!_linesByCaseAndTranscript.TryGetValue(caseId, out var byTranscript))
                {
                    return Array.Empty<TranscriptLine>();
                }

                var aggregate = new List<TranscriptLine>();
                foreach (var kvp in byTranscript)
                {
                    aggregate.AddRange(kvp.Value);
                }

                return new ReadOnlyCollection<TranscriptLine>(aggregate);
            }
        }

        public void AppendLine(TranscriptLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (string.IsNullOrWhiteSpace(line.CaseId))
            {
                throw new ArgumentException("Transcript line CaseId cannot be null or whitespace.", nameof(line.CaseId));
            }

            if (string.IsNullOrWhiteSpace(line.TranscriptId))
            {
                throw new ArgumentException("Transcript line TranscriptId cannot be null or whitespace.", nameof(line.TranscriptId));
            }

            lock (_syncRoot)
            {
                if (!_linesByCaseAndTranscript.TryGetValue(line.CaseId, out var byTranscript))
                {
                    byTranscript = new Dictionary<string, List<TranscriptLine>>(StringComparer.OrdinalIgnoreCase);
                    _linesByCaseAndTranscript[line.CaseId] = byTranscript;
                }

                if (!byTranscript.TryGetValue(line.TranscriptId, out var list))
                {
                    list = new List<TranscriptLine>();
                    byTranscript[line.TranscriptId] = list;
                }

                list.Add(line);
            }
        }

        public void ClearTranscript(string caseId, string transcriptId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            if (string.IsNullOrWhiteSpace(transcriptId))
            {
                throw new ArgumentException("TranscriptId cannot be null or whitespace.", nameof(transcriptId));
            }

            lock (_syncRoot)
            {
                if (!_linesByCaseAndTranscript.TryGetValue(caseId, out var byTranscript))
                {
                    return;
                }

                if (!byTranscript.Remove(transcriptId))
                {
                    return;
                }

                if (byTranscript.Count == 0)
                {
                    _linesByCaseAndTranscript.Remove(caseId);
                }
            }
        }
    }
}

