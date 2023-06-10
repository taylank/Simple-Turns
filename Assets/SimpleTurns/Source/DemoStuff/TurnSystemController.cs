using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Innerverse.SimpleTurns
{
    public class TurnSystemController : MonoBehaviour
    {
        private TurnManager turnManager;

        private void Start()
        {
            this.turnManager = new TurnManager(TurnOrder.PingPong);
            var part1 = new TestParticipant(teamId: 1);
            var part2 = new TestParticipant(teamId: 2);
            var part3 = new TestParticipant(teamId: 2);

            try
            {
                this.turnManager.RegisterParticipant(part1);
                this.turnManager.RegisterParticipant(part2);
                this.turnManager.RegisterParticipant(part3);
                this.turnManager.AddTurnEventListener(new RoundListener());
                this.turnManager.Begin();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.turnManager.EndTurn();
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                this.turnManager.SkipParticipant();
            }
        }
    }

    public class RoundListener : ITurnEventListener
    {
        public Task OnTurnEvent(TurnEvent e)
        {
            if (e.TurnEventType != TurnEventType.EndRound && e.TurnEventType != TurnEventType.StartRound)
                return Task.CompletedTask;
            Debug.Log(e.TurnEventType + " " + e.RoundNumber);
            return Task.Delay(1000);
        }

        public int EventResponsePriority { get; set; }
    }

    public class TestParticipant : ParticipantBase
    {
        protected override Task OnTurnStart(TurnEvent e)
        {
            Debug.Log($"Participant {this.ParticipantId} starting turn.");
            return base.OnTurnStart(e);
        }

        protected override Task OnTurnEnd(TurnEvent e)
        {
            Debug.Log($"Participant {this.ParticipantId} ending turn.");
            return base.OnTurnEnd(e);
        }

        protected override Task OnSkipToNextParticipant(TurnEvent e)
        {
            Debug.Log($"Skipped to Participant {e.Participant.ParticipantId}.");
            return base.OnTurnEnd(e);
        }

        protected override Task OnTeamStart(TurnEvent e)
        {
            Debug.Log($"Team {e.Participant.TeamId} starting turn.");
            return base.OnTeamStart(e);
        }

        protected override Task OnTeamEnd(TurnEvent e)
        {
            Debug.Log($"Team {e.Participant.TeamId} ending turn.");
            return base.OnTeamEnd(e);
        }

        public TestParticipant(uint teamId) : base(teamId)
        {
        }
    }
}