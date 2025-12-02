using System;
using System.Collections.Generic;

namespace Neuma.Core.Cases
{
    public sealed class CaseProgressDefinition
    {
        public string CaseId { get; }
        public IReadOnlyList<string> RequiredRelationIds { get; }
        public int MinimumContradictionsToSubmit { get; }

        public CaseProgressDefinition(string caseId, IReadOnlyList<string> requiredRelationIds, int minimumContradictionsToSubmit)
        {
            if (string.IsNullOrWhiteSpace(caseId))
            {
                throw new ArgumentException("CaseId cannot be null or whitespace.", nameof(caseId));
            }

            CaseId = caseId;
            RequiredRelationIds = requiredRelationIds ?? throw new ArgumentNullException(nameof(requiredRelationIds));
            MinimumContradictionsToSubmit = minimumContradictionsToSubmit;
        }
    }
}

