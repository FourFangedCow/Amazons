using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using AmazonAIBase;

public class GameManager : MonoBehaviour {

    public GameInst GameInstance = new GameInst();
    int Width = 10;
	int Height = 10;
	int NumberOfPlayers = 2;
	int NumberOfAmazons = 1;
	public GameObject TileInst;
	public GameObject AmazonInst;
	public GameObject ArrowInst;


	public void SetWidth(string width) { int.TryParse(width, out Width); }
	public void SetHeight(string height) { int.TryParse (height, out Height); }

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


    public void StartGame() {
		//CALCULATE OFFSET
		Vector3 offset = new Vector3((((Width - 1) * 1.1f) + 0.2f)/ 2.0f, 0, (((Height - 1) * 1.1f) + 0.2f) / 2.0f);
		//DRAW BOARD
		List<List<int>> newBoard = new List<List<int>>(Width);
		List<List<GameObject>> newTiles = new List<List<GameObject>>(Width);
		for (int x = 0; x < Width; ++x) {
			newTiles.Add(new List<GameObject>(Height));
			newBoard.Add(new List<int>(Height));
			for(int y = 0; y < Height; ++y) {
				newBoard[x].Add(0);
				newTiles[x].Add((GameObject)Instantiate(TileInst, new Vector3((x + ((x + 1) * 0.1f)),0 , (y + 1 + ((y + 1) * 0.1f))) - offset, Quaternion.LookRotation(new Vector3(0, 1, 0))));
			}
		}
		GameInstance.SetBoard(newTiles, newBoard, Width, Height);
		//ADD PLAYERS
		Amazons.Player[] players = new Amazons.Player[NumberOfPlayers];
		for (int i = 0; i < NumberOfPlayers; ++i) {
			players[i] = new Amazons.Player(i, GameInstance);
			players[i].Pawns = new List<Amazons.Pawn>();
		}
		//ADD AMAZONS
		System.Random rand = new System.Random();
		foreach(var player in players) {
			for(int i = 0; i < NumberOfAmazons; ++i) {
				//CALCULATE POSITION
				Point pos = new Point(rand.Next(0, Width), rand.Next(0, Height));
				GameObject newAmazon = (GameObject)Instantiate(AmazonInst, GetVectorFromPoint(pos), Quaternion.identity); 
				player.Pawns.Add (new Amazon(newAmazon, i));
			}
		}
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

	Vector3 GetVectorFromPoint(Point point) {
		return (GameInstance.Tiles[point.X][point.Y].GetComponent<Transform>().position + new Vector3(0, 0.5f, 0));
	}
}

public class Amazon : Amazons.Pawn {
	public GameObject Pawn;
	public Amazon(GameObject pawn, int id) {
		ID = id;
		Pawn = pawn;
	}
}

public class GameInst : Amazons.Game {
	public List<List<GameObject>> Tiles;
	public void SetBoard(List<List<GameObject>> tiles, List<List<int>> newBoard, int width, int height) {
		Tiles = tiles;
        Board = newBoard;
        Height = height;
        Width = width;
    }
}
