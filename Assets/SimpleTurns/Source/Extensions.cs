using System.Collections.Generic;
using System.Linq;

namespace Innerverse.SimpleTurns
{
    public static class Extensions
    {
        public static void AddParticipant(this Dictionary<uint, List<IParticipant>> collection,
            IParticipant participant, bool insertAsFirst = false)
        {
            if (collection.TryGetValue(participant.TeamId, out var team))
            {
                // If team already contains this participant, nothing to do here
                if (team.Contains(participant)) return;
                
                // Otherwise add it to the team
                if (insertAsFirst)
                {
                    team.Insert(0, participant);
                }
                else
                {
                    team.Add(participant);
                }
            }
            else
            {
                // A team does not already exist for this participant's teamId. Create one and add the participant to it
                collection.Add(participant.TeamId, new List<IParticipant> {participant});
            }
        }

        public static void RemoveParticipant(this Dictionary<uint, List<IParticipant>> collection,
            IParticipant participant)
        {
            foreach (var team in collection.Values)
            {
                team.Remove(participant);
            }
        }
        
        public static IParticipant FindParticipant(this Dictionary<uint, List<IParticipant>> collection,
            uint participantId)
        {
            foreach (var team in collection.Values)
            {
                var p = team.FirstOrDefault(p => p.ParticipantId == participantId);
                if (p != null) return p;
            }

            return null;
        }

        public static List<IParticipant> ParticipantsToList(this Dictionary<uint, List<IParticipant>> collection)
        {
            var result = new List<IParticipant>();
            foreach (var team in collection.Values)
            {
                result.AddRange(team);
            }

            return result;
        }

        public static void ReverseTeams(this Dictionary<uint, List<IParticipant>> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                var element = collection.ElementAt(i);
                element.Value.Reverse();
            }
        }
    }
}