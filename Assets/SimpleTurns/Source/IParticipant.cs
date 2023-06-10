namespace Innerverse.SimpleTurns
{
    public interface IParticipant : ITurnEventListener
    {
        uint ParticipantId { get; set; }
        uint TeamId { get; set; }
        int GetInitiative();
    }
}