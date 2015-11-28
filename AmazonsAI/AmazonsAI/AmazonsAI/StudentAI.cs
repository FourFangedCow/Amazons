using System;
using System.Collections.Generic;
using System.Text;
using AmazonAIBase;

namespace AmazonsAI
{
    public class SecondFillerBot: AmazonAIBase.AIBase
    {
        System.Random rand;
        public SecondFillerBot()
        {
            StudentName = "Second Filler Bot";
            rand = new System.Random();
        }

        override public Amazons.Move YourTurn()
        {
            Amazons.Move move = new Amazons.Move();
            List<List<int>> board = Owner.GetBoard_Simple();
            foreach(var pawn in Owner.Pawns) {
                for (int x = -1; x < 2; x++) {
                    for (int y = -1; y < 2; y++) {
                        if(CheckOutOfBounds(new Point(pawn.Position.X + x, pawn.Position.Y + y))) {
                            if (board[pawn.Position.X + x][pawn.Position.Y + y] == 0)
                            {
                                DebugPrint("ASDF");
                                move.ID = pawn.ID;
                                move.MoveTo = new Point(pawn.Position.X + x, pawn.Position.Y + y);
                                move.ShootTo = new Point(pawn.Position.X, pawn.Position.Y);
                                return move;
                            }
                        }
                    }
                }
            }
            rand = new System.Random();
            move.ID = rand.Next(0, Owner.Pawns.Count - 1);
            move.MoveTo = new Point(Owner.Pawns[move.ID].Position.X + 1, Owner.Pawns[move.ID].Position.Y + 1);
            move.ShootTo = new Point(Owner.Pawns[move.ID].Position.X, Owner.Pawns[move.ID].Position.Y);
            return move;
            //return new Amazons.Move();
        }
        
        private bool CheckOutOfBounds(Point point) {
            if (point.X < Owner.GameInstance.Width && point.X >= 0 &&
                point.Y < Owner.GameInstance.Height && point.Y >= 0)
                return true;
            return false;

        }
        List<Amazons.Pawn> GetEnemyPawns() {
            List<Amazons.Pawn> enemyPawns = new List<Amazons.Pawn>();
            foreach(var player in Owner.GameInstance.Players) {
                if (player.ID != Owner.ID) {
                    foreach (var pawn in player.Pawns) {
                        enemyPawns.Add(pawn);
                    }
                }
            }
            return enemyPawns;
        }
    }
}
