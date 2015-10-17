using System;
using System.Collections.Generic;
using System.Text;

public class Point {
    public int X = 0;
    public int Y = 0;

    public Point(int x, int y) {
        X = x;
        Y = y;
    }

    public static bool operator !=(Point lhs, Point rhs) { return (lhs.X != rhs.X) || (lhs.Y != rhs.Y); }
    public static bool operator ==(Point lhs, Point rhs) { return (lhs.X == rhs.X) && (lhs.Y == rhs.Y); }
}

namespace Amazons {
    public class Move {
        public int ID { get; set; }
        public Point MoveTo { get; set; }
        public Point ShootTo { get; set; }
        public Move() { }
        public Move(int id, Point move, Point shoot) {
            ID = id;
            MoveTo = move;
            ShootTo = shoot;
        }
    }
    public class Pawn {
        public int ID  { get; protected set; }
        public Point Position;
        private Stack<Move> History;

        public Pawn() { ID = -1; }
    }

    public class Player {
        public int ID { get; protected set; }
        public List<Pawn> Pawns;
        public AmazonAIBase.AIBase AI;

        private Game GameInstance;
        
        public Player(int id, Game game) {
            ID = id;
            GameInstance = game;
        }

        public int GetIDByPoint(Point point) {
            foreach (var pawn in Pawns)
                if (pawn.Position == point)
                    return pawn.ID;
            return -1;
        }

        public List<List<int>> GetBoard() { return GameInstance.Board; }
        public List<List<int>> GetBoard_Simple() {
            var ComplexBoard = GetBoard();
            var SimpleBoard = new List<List<int>>();
            for(int x = 0; x < GameInstance.Width; ++x)
                for(int y = 0; y < GameInstance.Height; ++y)
                    SimpleBoard[x][y] = ComplexBoard[x][y] > 0 ? (ComplexBoard[x][y] != ID ? 2 : 1) : ComplexBoard[x][y];
            return SimpleBoard;
        }
    }
    public class Game {
        // -1 = Fire, 0 = Empty, Anything else = Pawn
        public List<List<int>> Board { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public Player[] Players { get; protected set; }
        private Stack<Pawn> History;

        public Game() {}
    }
}

namespace AmazonAIBase {

    public class AIBase {
        public string StudentName { get; protected set; }
        private Amazons.Player Owner;
        virtual public Amazons.Move YourTurn() { return new Amazons.Move(); }
    }
}
