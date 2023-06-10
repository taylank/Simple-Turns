namespace Innerverse.SimpleTurns
{
    public struct TurnEvent
    {
        public readonly TurnEventType TurnEventType;
        public readonly IParticipant Participant;
        public readonly int RoundNumber;

        public TurnEvent(TurnEventType turnEventType, int roundNumber, IParticipant participant)
        {
            this.TurnEventType = turnEventType;
            this.Participant = participant;
            this.RoundNumber = roundNumber;
        }
    }
    
    public enum TurnEventType
    {
        StartRound,
        EndRound,
        StartTeam,
        EndTeam,
        StartTurn,
        EndTurn,
        SkipToNextParticipant
    }
}