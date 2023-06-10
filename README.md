# SimpleTurns
 An API only library for managing turns in a turn based game. It only deals with managing the turn order, and executing various turn events (begin turn, end turn, begin/end round, etc.). In order to make it as unopinionated as possible, it does not concern itself with any other trappings of the turn based games genre (move tracking, stats, grids, etc).
 
 You are welcome to use it or extend it for your games. If you notice any bugs, or possible improvements, I would welcome PRs.
 
 
Features:
- Can handle an arbitrary number of teams
- Allows for different styles of turn order (e.g. initiative based, ping-pong, regular)
- Has an event mechanism to notify subscribers when:
  - Turn starts or ends
  - Round starts or ends
  - Turn switches to another team
  - When player switches to a new participant (without ending another one's turn. Think choosing a team member in a tactics game)
- Async event listener execution with sorting by priority   


Core Parts
- TurnManager: Handles all the book keeping and event execution.
- IParticipant: An entity that can play a turn
- ITurnEventListener: Anything that needs to listen to various turn system events. IParticipant inherits from this, so no need to add them twice.
- ParticipantBase: A vanilla C# base class for a participant, for convenience.
- MonoParticipant: Same as ParticipantBase but implemented as a MonoBehaviour.

TurnManager Public Methods
```
// Starts the whole turn tracking system
async Task Begin(); 

 // Add a new participant to the system. They are assigned a unique parcipantid when you do this
async Task RegisterParticipant(IParticipant participant);

 // Remove participant from the system
async Task UnregisterParticipant(uint participantId);

 // Add a listener for all turn events. You can filter for the relevant ones in your implementations
void AddTurnEventListener(ITurnEventListener turnEventListener);

// Self-explanatory
void RemoveTurnEventListener(ITurnEventListener listener);

// Ends the turn of the current participant, triggers a TurnEnd event, and moves on 
// to the next participant when all listeners have finished executing
async Task EndTurn(); 

// Skip to the next participant on a team without ending the turn of the previous one: 
// i.e. changing turn order. Not allowed for initiative based turn order.
async Task SkipParticipant(); 

// Jump to a particular participant in the turn order
async Task SkipToParticipant(uint participantId);

// Returns a data container that can be saved and later used to rebuild the game state.
// Need to add a bit more code to make this fully functional, but you can add that yourself too.
TurnManagerState GetStateData(); 
```


Usage:
```
// Initialize a new instance of the TurnManager class by specifying a turn order style
var turnManager = new TurnManager(TurnOrder.PingPong);

// Add a few participants. TestParticipant is just a demo class that inherits from ParticipantBase
var participant1 = new TestParticipant(teamId: 1);
var participant2 = new TestParticipant(teamId: 2);
var participant3 = new TestParticipant(teamId: 2);

// Add some event listeners. To do some UI transitions, reset moves, stats, etc.
this.turnManager.AddTurnEventListener(new TestRoundListener());
// Run the system
this.turnManager.Begin();
```

Sample Participant Class:
```
 public class TestParticipant : ParticipantBase
    {
        protected override Task OnTurnStart(TurnEvent e)
        {
            // Do an audio bark, show some fancy animation, initialize moves, etc.
            Debug.Log($"Participant {this.ParticipantId} starting turn.");
            return base.OnTurnStart(e);
        }

        protected override Task OnTurnEnd(TurnEvent e)
        {
            // Apply damage over time, drain all remaining moves, etc...
            Debug.Log($"Participant {this.ParticipantId} ending turn.");
            return base.OnTurnEnd(e);
        }

        protected override Task OnSkipToNextParticipant(TurnEvent e)
        {
            // This is called when you switch to a new entity on the player team.
            // You might move the camera to focus on that entity or something
            Debug.Log($"Skipped to Participant {e.Participant.ParticipantId}.");
            return base.OnTurnEnd(e);
        }

        protected override Task OnTeamStart(TurnEvent e)
        {
            // Do some fancy transition. "TEAM AWESOME - GO!"
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
 ```
