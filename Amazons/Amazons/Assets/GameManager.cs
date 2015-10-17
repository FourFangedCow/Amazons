using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using AmazonAIBase;

public class GameManager : MonoBehaviour {

    public GameInst GameInstance = new GameInst();
    int Width;
    int Height;

    // Use this for initialization
    void Start () {
        string[] aiLibs = Directory.GetFiles(@".\AI\", "*.dll");
        foreach (string path in aiLibs) {
            Assembly asm = Assembly.LoadFrom(path);
            Type[] types = asm.GetTypes();
            object instance = Activator.CreateInstance(types[0]);
            AmazonAIBase.AIBase temp = (AmazonAIBase.AIBase)instance;
            GameObject.Find("NameText").GetComponent<Text>().text = temp.StudentName;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    void StartGame() {
        List<List<int>> newBoard = new List<List<int>>(Width);
        for(int y = 0; y < Height; ++y)
            newBoard[y] = new List<int>(Height);
        GameInstance.SetBoard(newBoard, Width, Height);
    }
	
	bool ActivateAI(Amazons.Player player) {
		Amazons.Move move = player.AI.YourTurn();
		//GameInstance.Board
		if(!CheckLine(player.Pawns[move.ID].Position, move.MoveTo) || 
		   !CheckLine(move.MoveTo, move.ShootTo))
			return false; // ACTIVE PLAYER LOSES
			
		player.Pawns[move.ID].Position = move.MoveTo;		// Set pawn pos to correct pos
		SetBoardPoint(player.Pawns[move.ID].Position, 0); 	// Set old pos to empty
		SetBoardPoint(move.MoveTo, move.ID + player.ID); 	// Set new pos to id
		SetBoardPoint(move.ShootTo, -1);					// Set shoot pos to -1
		
		ActivateVisualMove(move);
		return true;
	}
	
	void ActivateVisualMove(Amazons.Move move) {
		// Activate game object, move squares, call event
	}
		
	bool CheckLine(Point start, Point end) {
		if(((start.X - end.X) - (start.Y - end.Y)) == 0 &&
			CheckValidPoint(start) && CheckValidPoint(end)) {
			int h = (end.X - start.X) > 0 ? 1 : (end.X - start.X) < 0 ? -1 : 0;
			int v = (end.Y - start.Y) > 0 ? 1 : (end.Y - start.Y) < 0 ? -1 : 0;
			int x = start.X; int y = start.Y;
			while(x != end.X || y != end.Y) {
				if(GameInstance.Board[x][y] != 0)
					return false;
				x += v; y += h;
			}
			return true;
		}
		return false;
	}
	
	bool CheckValidPoint(Point point) {
		return (point.X < Width && point.X >= 0 && point.Y < Height && point.Y >= 0);
	}
	
	void SetBoardPoint(Point point, int type) {
		GameInstance.Board[point.X][point.Y] = type;
	}
}

public class GameInst : Amazons.Game {
    public void SetBoard(List<List<int>> newBoard, int width, int height) {
        Board = newBoard;
        Height = height;
        Width = width;
    }
}
