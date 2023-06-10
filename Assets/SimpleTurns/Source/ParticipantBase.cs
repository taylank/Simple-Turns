using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

namespace Innerverse.SimpleTurns
{
    public class ParticipantBase : IParticipant
    {
        public uint ParticipantId { get; set; }

        public uint TeamId { get; set; }

        public int EventResponsePriority { get; set; }
     
        public virtual async Task OnTurnEvent(TurnEvent e)
        {
            switch (e.TurnEventType)
            {
                case TurnEventType.StartRound:
                    await this.OnRoundStart(e);
                    break;
                case TurnEventType.EndRound:
                    await this.OnRoundEnd(e);
                    break;
                case TurnEventType.StartTurn:
                    if (e.Participant.ParticipantId == this.ParticipantId) await this.OnTurnStart(e);
                    break;
                case TurnEventType.EndTurn:
                    if (e.Participant.ParticipantId == this.ParticipantId) await this.OnTurnEnd(e);
                    break;
                case TurnEventType.StartTeam:
                    if (e.Participant.TeamId == this.TeamId) await this.OnTeamStart(e);
                    break;
                case TurnEventType.EndTeam:
                    if (e.Participant.TeamId == this.TeamId) await this.OnTeamEnd(e);
                    break;
                case TurnEventType.SkipToNextParticipant:
                    await this.OnSkipToNextParticipant(e);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ParticipantBase(uint teamId)
        {
            this.TeamId = teamId;
        }
        
        protected virtual Task OnTeamStart(TurnEvent e)
        {
            return Task.CompletedTask;
        }
        
        protected virtual Task OnTeamEnd(TurnEvent e)
        {
            return Task.CompletedTask;
        }
        
        protected virtual Task OnSkipToNextParticipant(TurnEvent e)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnTurnStart(TurnEvent e)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnTurnEnd(TurnEvent e)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnRoundStart(TurnEvent e)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnRoundEnd(TurnEvent e)
        {
            return Task.CompletedTask;
        }
        
        public virtual int GetInitiative()
        {
            return Random.Range(1, 11);
        }
    }
}