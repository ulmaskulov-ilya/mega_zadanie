using System.Text;

class Program
{
    private record State(string Corridor, string Rooms);

    private static readonly Dictionary<char, int> EnergyCosts = new()
    {
        { 'A', 1 }, { 'B', 10 }, { 'C', 100 }, { 'D', 1000 }
    };

    private static readonly Dictionary<char, int> TargetRoom = new()
    {
        { 'A', 0 }, { 'B', 1 }, { 'C', 2 }, { 'D', 3 }
    };

    private static readonly char[] TargetChar = ['A', 'B', 'C', 'D'];
    private static readonly int[] CrossPos = [2, 4, 6, 8];
    private static readonly int[] StopPositions = [0, 1, 3, 5, 7, 9, 10];


    private static int Solve(List<string> lines)
    {
        var roomDepth = lines.Count - 3;
        var (startState, goalState) = ProcessInput(lines, roomDepth);
        var minCosts = new Dictionary<State, int>();
        var pq = new PriorityQueue<State, int>();
        minCosts[startState] = 0;
        pq.Enqueue(startState, 0);

        while (pq.TryDequeue(out var currentState, out var currentCost))
        {
            if (currentState.Equals(goalState)) return currentCost;
            if (minCosts.TryGetValue(currentState, out var knownCost) && currentCost > knownCost) continue;

            foreach (var (nextState, moveCost) in GetNextMoves(currentState, roomDepth))
            {
                var newCost = currentCost + moveCost;
                if (!minCosts.TryGetValue(nextState, out var value) || newCost < value)
                {
                    minCosts[nextState] = newCost;
                    pq.Enqueue(nextState, newCost);
                }
            }
        }

        return -1;
    }

    private static (State, State) ProcessInput(List<string> lines, int roomDepth)
    {
        var roomChars = new char[4, roomDepth];
        for (var d = 0; d < roomDepth; d++)
        {
            var line = lines[d + 2];
            roomChars[0, d] = line[3];
            roomChars[1, d] = line[5];
            roomChars[2, d] = line[7];
            roomChars[3, d] = line[9];
        }
        var startRooms = new string[4];
        var goalRooms = new string[4];
        for (var r = 0; r < 4; r++)
        {
            var sb = new StringBuilder();
            for (var d = 0; d < roomDepth; d++)
                sb.Append(roomChars[r, d]);
            startRooms[r] = sb.ToString();
            goalRooms[r] = new string(TargetChar[r], roomDepth);
        }
        var startState = new State(new string('.', 11), string.Join(";", startRooms));
        var goalState = new State(new string('.', 11), string.Join(";", goalRooms));
        return (startState, goalState);
    }

    private static List<(State, int)> GetNextMoves(State currentState, int roomDepth)
    {
        var moves = new List<(State, int)>();
        var corridor = currentState.Corridor;
        var rooms = currentState.Rooms.Split(';');

        for (var h = 0; h < corridor.Length; h++)
        {
            var charPos = corridor[h];
            if (charPos == '.') continue;
            var destRoomIndex = TargetRoom[charPos];
            var destRoom = rooms[destRoomIndex];
            var destHallPos = CrossPos[destRoomIndex];

            if (!destRoom.All(c => c == '.' || c == charPos)) continue;
            if (!IsCorridorClear(corridor, h, destHallPos)) continue;

            var destDepth = -1;
            for (var d = roomDepth - 1; d >= 0; d--)
            {
                if (destRoom[d] != '.') continue;
                destDepth = d;
                break;
            }

            if (destDepth == -1) continue;

            var cost = (Math.Abs(h - destHallPos) + destDepth + 1) * EnergyCosts[charPos];
            var newCorridor = corridor.ToCharArray();
            newCorridor[h] = '.';
            var newRooms = (string[])rooms.Clone();
            var newRoomChars = newRooms[destRoomIndex].ToCharArray();
            newRoomChars[destDepth] = charPos;
            newRooms[destRoomIndex] = new string(newRoomChars);

            moves.Add((new State(new string(newCorridor), string.Join(";", newRooms)), cost));
        }

        for (var r = 0; r < 4; r++)
        {
            var room = rooms[r];
            var target = TargetChar[r];

            var srcDepth = -1;
            var charToMove = '.';
            for (var d = 0; d < roomDepth; d++)
            {
                if (room[d] == '.') continue;
                srcDepth = d;
                charToMove = room[d];
                break;
            }

            if (charToMove == '.') continue;
            var shouldMove = false;
            if (charToMove == target)
            {
                for (var d = srcDepth + 1; d < roomDepth; d++)
                {
                    if (room[d] == target) continue;
                    shouldMove = true;
                    break;
                }
            }
            else shouldMove = true;

            if (!shouldMove) continue;
            var srcHallPos = CrossPos[r];
            foreach (var stopPos in StopPositions)
            {
                if (!IsCorridorClear(corridor, srcHallPos, stopPos)) continue;
                var cost = (Math.Abs(srcHallPos - stopPos) + srcDepth + 1) * EnergyCosts[charToMove];
                var newCorridor = corridor.ToCharArray();
                newCorridor[stopPos] = charToMove;
                var newRooms = (string[])rooms.Clone();
                var newRoomChars = newRooms[r].ToCharArray();
                newRoomChars[srcDepth] = '.';
                newRooms[r] = new string(newRoomChars);
                moves.Add((new State(new string(newCorridor), string.Join(";", newRooms)), cost));
            }
        }

        return moves;
    }

    private static bool IsCorridorClear(string corridor, int startPos, int endPos)
    {
        var move = endPos > startPos ? 1 : -1;
        for (var i = startPos + move;; i += move)
        {
            if (corridor[i] != '.') return false;
            if (i == endPos) break;
        }

        return true;
    }

    static void Main()
    {
        var lines = new List<string>();
        while (Console.ReadLine() is { } line)
            lines.Add(line);

        Console.WriteLine(Solve(lines));
    }
}