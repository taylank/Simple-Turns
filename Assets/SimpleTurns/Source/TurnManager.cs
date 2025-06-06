using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Innerverse.SimpleTurns
{
    public class TurnManager
    {
        public int CurrentRound { get; private set; }
        public IParticipant CurrentParticipant { get; private set; }
        private List<IParticipant> participantsToGo;
        private readonly Dictionary<uint, List<IParticipant>> teams;
        private readonly List<ITurnEventListener> turnEventListeners;

        private TurnEvent TurnStartEvent =>
            new TurnEvent(TurnEventType.StartTurn, this.CurrentRound, this.CurrentParticipant);

        private TurnEvent TurnEndEvent =>
            new TurnEvent(TurnEventType.EndTurn, this.CurrentRound, this.CurrentParticipant);

        private TurnEvent RoundStartEvent => new TurnEvent(TurnEventType.StartRound, this.CurrentRound, null);
        private TurnEvent RoundEndEvent => new TurnEvent(TurnEventType.EndRound, this.CurrentRound, null);

        private TurnEvent SkipParticipantEvent =>
            new TurnEvent(TurnEventType.SkipToNextParticipant, this.CurrentRound, this.CurrentParticipant);

        private TurnEvent TeamStartEvent =>
            new TurnEvent(TurnEventType.StartTeam, this.CurrentRound, this.CurrentParticipant);
        
        private TurnEvent TeamEndEvent => new TurnEvent(TurnEventType.EndTeam, this.CurrentRound, this.CurrentParticipant);
        private bool hasBegun;
        private readonly TurnOrder turnOrder;
        private uint maxId;

        public TurnManager(TurnOrder turnOrder)
        {
            this.CurrentRound = 0;
            this.turnOrder = turnOrder;
            this.turnEventListeners = new List<ITurnEventListener>();
            this.teams = new Dictionary<uint, List<IParticipant>>();
            this.participantsToGo = new List<IParticipant>();
        }

        private uint GetUniqueId()
        {
            this.maxId++;
            return this.maxId;
        }

        public async Task RegisterParticipant(IParticipant participant)
        {
            this.teams.AddParticipant(participant);
            // Assign a unique id to the participant
            participant.ParticipantId = this.GetUniqueId();
            var insertAt = this.participantsToGo.FindLastIndex(p => p.TeamId == participant.TeamId);

            if (insertAt < 0)
            {
                this.participantsToGo.Add(participant);

                if (this.hasBegun)
                {
                    this.CurrentParticipant = participant;
                    await this.ExecuteTurnEvent(this.TurnStartEvent);
                }
            }
            else
            {
                this.participantsToGo.Insert(insertAt + 1, participant);
            }

            this.AddTurnEventListener(participant);
        }

        public async Task UnregisterParticipant(uint participantId)
        {
            var participant = this.teams.FindParticipant(participantId);
            if (participant == null) return;

            if (participant == CurrentParticipant)
            {
                this.participantsToGo.Remove(participant);
                await this.ExecuteTurnEvent(this.TurnEndEvent);
            }

            this.teams.RemoveParticipant(participant);

            this.RemoveTurnEventListener(participant);
        }

        public async Task UnregisterParticipant(IParticipant participant)
        {
            await this.UnregisterParticipant(participant.ParticipantId);
        }

        public void AddTurnEventListener(ITurnEventListener turnEventListener)
        {
            if (this.turnEventListeners.Contains(turnEventListener)) return;
            this.turnEventListeners.Add(turnEventListener);
            this.turnEventListeners.Sort((e1, e2) => e2.EventResponsePriority.CompareTo(e1.EventResponsePriority));
        }

        public void RemoveTurnEventListener(ITurnEventListener listener)
        {
            if (listener is IParticipant participant && this.teams.FindParticipant(participant.ParticipantId) != null)
            {
                throw new InvalidOperationException(
                    "Removing an active participant from TurnEventListeners is not allowed.");
            }

            this.turnEventListeners.Remove(listener);
        }


        public async Task Begin()
        {
            this.PrepareForNextRound();
            this.hasBegun = true;
            await this.ExecuteTurnEvent(this.RoundStartEvent);
            this.CurrentParticipant = this.participantsToGo[0];
            await this.ExecuteTurnEvent(this.TurnStartEvent);
        }

        private void PrepareForNextRound()
        {
            this.participantsToGo.Clear();

            if (this.turnOrder == TurnOrder.FreeInitiative)
            {
                this.participantsToGo = this.teams.ParticipantsToList().OrderBy(p => p.GetInitiative()).ToList();
            }
            else
            {
                switch (this.turnOrder)
                {
                    case TurnOrder.PingPong:
                    {
                        // Don't reverse the order for the first round 
                        if (this.CurrentRound > 0) this.teams.ReverseTeams();
                        break;
                    }
                    case TurnOrder.Default:
                    {
                        break;
                    }
                }
                
                this.participantsToGo = this.teams.ParticipantsToList();
            }

            this.CurrentRound++;
        }

        public async Task EndTurn()
        {
            var currentTeam = this.CurrentParticipant.TeamId;
            
            // Mark participant as having played
            this.participantsToGo.Remove(this.CurrentParticipant);

            // OnTurnEnd
            await this.ExecuteTurnEvent(this.TurnEndEvent);
            
            // if all participants have played, end round
            if (this.participantsToGo.Count == 0)
            {
                await this.EndRound();
            }
            
            // See if the current team is done
            if (this.participantsToGo.All(p => p.TeamId != currentTeam))
            {
                await this.ExecuteTurnEvent(this.TeamEndEvent);
            }

            if (this.participantsToGo.Count == 0)
            {
                throw new Exception("No active participants left in turn system. Cannot proceed.");
            }

            // Start new turn
            this.CurrentParticipant = this.participantsToGo[0];
            await this.ExecuteTurnEvent(this.TeamStartEvent);

            await this.ExecuteTurnEvent(this.TurnStartEvent);
        }

        private async Task EndRound()
        {
            // OnRoundEnd
            await this.ExecuteTurnEvent(this.RoundEndEvent);

            this.PrepareForNextRound();

            // OnRoundStart
            await this.ExecuteTurnEvent(this.RoundStartEvent);
        }

        public async Task SkipParticipant()
        {
            if (this.turnOrder == TurnOrder.FreeInitiative)
                throw new InvalidOperationException("Cannot skip participants in FreeInitiative mode");

            if (this.CurrentParticipant == null) return;

            // Move the current participant to the end of the team order
            var currentTeam = this.CurrentParticipant.TeamId;

            // Only proceed if there is another participant on the same team to skip to
            if (this.participantsToGo.Count(p => p.TeamId == currentTeam) <= 1)
            {
                return;
            }

            this.participantsToGo.Remove(this.CurrentParticipant);
            var lastIndexOfTeam = this.participantsToGo.FindLastIndex(p => p.TeamId == currentTeam);

            this.participantsToGo.Insert(lastIndexOfTeam + 1, this.CurrentParticipant);
            
            // Reorder the teams as well so that the order change will persist between turns.
            this.teams.RemoveParticipant(this.CurrentParticipant);
            this.teams.AddParticipant(this.CurrentParticipant);

            this.CurrentParticipant = this.participantsToGo[0];

            await this.ExecuteTurnEvent(new TurnEvent(TurnEventType.SkipToNextParticipant, this.CurrentRound,
                this.CurrentParticipant));
        }

        public async Task SkipToParticipant(uint participantId)
        {
            if (this.turnOrder == TurnOrder.FreeInitiative)
                throw new InvalidOperationException("Cannot skip participants in FreeInitiative mode");

            if (this.CurrentParticipant == null) return;

            var targetParticipant = this.participantsToGo.Find(p => p.ParticipantId == participantId);
            if (targetParticipant == null) return;
            this.participantsToGo.Remove(targetParticipant);
            this.participantsToGo.Insert(0, targetParticipant);
            this.teams.RemoveParticipant(targetParticipant);
            this.teams.AddParticipant(targetParticipant, true);
            
            this.CurrentParticipant = this.participantsToGo[0];

            await this.ExecuteTurnEvent(new TurnEvent(TurnEventType.SkipToNextParticipant, this.CurrentRound,
                this.CurrentParticipant));
        }
        
        private async Task ExecuteTurnEvent(TurnEvent e)
        {
            if (!this.hasBegun) return;
            this.turnEventListeners.RemoveAll(t => t == null);

            for (var i = 0; i < this.turnEventListeners.Count; i++)
            {
                await this.turnEventListeners[i].OnTurnEvent(e);
            }
        }

        public TurnManagerState GetStateData()
        {
            var teamData = new Dictionary<uint, List<uint>>();
            foreach (var team in this.teams)
            {
                teamData.Add(team.Key, team.Value.Select(p => p.ParticipantId).ToList());
            }

            return new TurnManagerState()
            {
                CurrentParticipantId = this.CurrentParticipant.ParticipantId,
                CurrentRound = this.CurrentRound,
                MaxId = this.maxId,
                ParticipantsToGo = this.participantsToGo.Select(p => p.ParticipantId).ToList(),
                Teams = teamData,
                TurnOrderType = this.turnOrder
            };
        }
    }

    public enum TurnOrder
    {
        Default, // Grouping by team
        PingPong, // Participants reverse order within teams, so that the last participant becomes first
        FreeInitiative, // Participants are grouped by initiative with mixed team order  
    }
}