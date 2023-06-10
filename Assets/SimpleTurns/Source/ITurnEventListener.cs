using System.Threading.Tasks;

namespace Innerverse.SimpleTurns
{
    public interface ITurnEventListener
    {
        public Task OnTurnEvent(TurnEvent e);
        public int EventResponsePriority { get; }
    }
}