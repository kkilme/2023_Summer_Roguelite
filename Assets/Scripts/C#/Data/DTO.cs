using System;

namespace DTO
{
    [Serializable]
    public class IncrementBalanceParam
    {
        public string currencyId;
        public string configAssignmentHash;

        public IncrementBalanceParam(string currencyId, string configAssignmentHash)
        {
            this.currencyId = currencyId;
            this.configAssignmentHash = configAssignmentHash;
        }

        public override string ToString()
        {
            return $"{{\"currencyId\": @currencyId, \"configAssignmentHash\": @configAssignmentHash }}";
        }
    }
}
