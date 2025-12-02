namespace Neuma.Core.Cases
{
    public interface ICaseRepository
    {
        CaseDefinition Get(string caseId);
        bool TryGet(string caseId, out CaseDefinition? caseDefinition);
    }
}

