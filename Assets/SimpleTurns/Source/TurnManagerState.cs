using System.Collections.Generic;

namespace Innerverse.SimpleTurns
{
    [System.Serializable]
    public class TurnManagerState
    {
        public uint CurrentParticipantId { get; set; }
        public int CurrentRound { get; set; }
        public uint MaxId { get; set; }
        public Dictionary<uint, List<uint>> Teams { get; set; }
        public List<uint> ParticipantsToGo { get; set; }
        public TurnOrder TurnOrderType { get; set; }
    }
}