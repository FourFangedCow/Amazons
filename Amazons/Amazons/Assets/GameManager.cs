using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Threading;
using System;
using AmazonAIBase;





public class GameManager : MonoBehaviour {
    // GAME INSTANCE SHIT
    public GameInst GameInstance = new GameInst();
    bool GameRunning = false;
    int Width = 10;
	int Height = 10;
	int NumberOfPlayers = 3;
	int NumberOfAmazons = 4;
	public GameObject TileInst;
	public GameObject AmazonInst;
	public GameObject ArrowInst;
    public GameObject ToggleInst;
    public GameObject RedXInst;
    public GameObject SpearInst;
	Text Console;
    // MOVING ACTOR
    Amazons.Move CurrentMove;
    Amazons.Player CurrentPlayer;
    int CurrentPlayerIndex = 1;
    bool MoveActor = false;
    float MoveTimer = 0.0f;
    Vector3 StartingPos;
    Vector3 EndPos;
    GameObject MovingActor;

    // COLORS
    Color EmptyTile = new Color(1, 1, 1);
    Color ArrowTile = new Color(1, 0, 0);
    List<Color> PlayerTile = new List<Color>();

    // AI STUFF
    List<AmazonAIBase.AIBase> AllAI = new List<AmazonAIBase.AIBase>();
    List<AmazonAIBase.AIBase> ActiveAI = new List<AmazonAIBase.AIBase>();

	public void SetWidth(string width) { int.TryParse(width, out Width); }
	public void SetHeight(string height) { int.TryParse (height, out Height); }

    // Use this for initialization
    void Start () {
        int offset = 0;
        int i = 0;
        Transform canvas = GameObject.Find("Canvas").GetComponent<Transform>();
        string[] aiLibs = Directory.GetFiles(@".\AI\", "*.dll");
        foreach (string path in aiLibs) {
            print(path);
            Assembly asm = Assembly.LoadFrom(path);
            Type[] types = asm.GetTypes();
            object instance = Activator.CreateInstance(types[0]);
            AmazonAIBase.AIBase temp = (AmazonAIBase.AIBase)instance;
            AllAI.Add(temp);
            GameObject toggle = (GameObject)Instantiate(ToggleInst, new Vector3(-300, 200 + offset, 0), Quaternion.identity);
            toggle.GetComponent<Transform>().SetParent(canvas, false);
            toggle.GetComponentInChildren<Text>().text = temp.StudentName;
            toggle.GetComponent<AiToggle>().ID = i++;
            offset -= 40;
        }
		Console = GameObject.Find ("ConsoleArea").GetComponent<Text>();
        PlayerTile.Add(new Color(0, 1, 0));
        PlayerTile.Add(new Color(0, 0, 1));
        PlayerTile.Add(new Color(0, 1, 1));
        PlayerTile.Add(new Color(1, 1, 0));
        PlayerTile.Add(new Color(1, 0, 1));
    }
	
	// Update is called once per frame
	void Update () {
        if (GameRunning) {
            if (MoveActor)  {
                MoveTimer += (Time.deltaTime / 0.2f);
                if (MoveTimer > 1)
                {
                    MoveActor = false;
                    if (CurrentPlayerIndex >= NumberOfPlayers)
                        CurrentPlayerIndex = 0;
					Console.text = GameInstance.Players[CurrentPlayerIndex].AI.GetDebugString();
                    ActivateAI(GameInstance.Players[CurrentPlayerIndex++]);
                }
                MovingActor.GetComponent<Transform>().position = Vector3.Lerp(StartingPos, EndPos, MoveTimer);
            }
        }
	}


    public void StartGame() {
        GameObject.Destroy(GameObject.Find("Canvas"));
        NumberOfPlayers = ActiveAI.Count;
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
            players[i].AI = ActiveAI[i];
            players[i].AI.Owner = players[i];
		}
        
        GameInstance.SetPlayers(players);
		//ADD AMAZONS
		System.Random rand = new System.Random();
		foreach(var player in players) {
			for(int i = 0; i < NumberOfAmazons; ++i) {
				//CALCULATE POSITION
				Point pos = new Point(rand.Next(0, Width), rand.Next(0, Height));
				GameObject newAmazon = (GameObject)Instantiate(AmazonInst, GetVectorFromPoint(pos), Quaternion.identity);
                SetTile(pos, player.ID + 1);
				player.Pawns.Add (new Amazon(newAmazon, i, pos));
			}
		}

        GameRunning = true;
        ActivateAI(GameInstance.Players[0]);
    }
	
	bool ActivateAI(Amazons.Player player) {
        //Amazons.Move move = player.AI.YourTurn();
        //GameInstance.Board
        CurrentPlayer = player;


        CallWithTimeout(ActivateCurrentPlayer, 10000);
        //ActivateCurrentPlayer();

        Amazons.Move move = CurrentMove;

        SetBoardPoint(player.Pawns[move.ID].Position, 0);   // Set old pos to empty

        if (!CheckLine(player.Pawns[move.ID].Position, move.MoveTo) ||
           !CheckLine(move.MoveTo, move.ShootTo) || move.MoveTo == move.ShootTo) {
            GameRunning = false;
            print("FAIL MOVE");
            return false; // ACTIVE PLAYER LOSES
        }

        //player.Pawns[move.ID].Position = move.MoveTo;		// Set pawn pos to correct pos
        //SetBoardPoint(move.MoveTo, move.ID + player.ID); 	// Set new pos to id
        //SetBoardPoint(move.ShootTo, -1);					// Set shoot pos to -1


        player.Pawns[move.ID].Position = move.MoveTo;		// Set pawn pos to correct pos
        SetTile(player.Pawns[move.ID].Position, 0);
        SetTile(move.MoveTo, player.ID + 1); 	                // Set new pos to id
        SetTile(move.ShootTo, -1);					        // Set shoot pos to -1

        // SET RED X
        Instantiate(RedXInst, GetVectorFromPoint(move.ShootTo) + new Vector3(0, -0.74f, 0), Quaternion.LookRotation(new Vector3(0, 1, 0)));
        Instantiate(SpearInst, GetVectorFromPoint(move.ShootTo) + new Vector3(0.09f, -0.38f, 0), Quaternion.identity);

        print("COMPLETE MOVE");
        ActivateVisualMove(player, move);
		return true;
	}
	
	void ActivateVisualMove(Amazons.Player player, Amazons.Move move) {
        MovingActor = ((Amazon)player.Pawns[move.ID]).Pawn;
        Transform t = MovingActor.GetComponent<Transform>();
        MoveActor = true;
        MoveTimer = 0;
        StartingPos = t.position;
        EndPos = GetVectorFromPoint(move.MoveTo);
	}
	
    void SetTile(Point point, int id) {
        GameInstance.Board[point.X][point.Y] = id;
        switch(id)
        {
            case -1:
                GameInstance.Tiles[point.X][point.Y].GetComponent<SpriteRenderer>().color = ArrowTile;
                break;
            case 0:
                GameInstance.Tiles[point.X][point.Y].GetComponent<SpriteRenderer>().color = EmptyTile;
                break;
            default:
                GameInstance.Tiles[point.X][point.Y].GetComponent<SpriteRenderer>().color = PlayerTile[id - 1];
                break;
        }
    }

    bool CheckLine(Point start, Point end) {
        if (((Math.Abs(start.X - end.X) - Math.Abs(start.Y - end.Y)) == 0 ||
            (start.X - end.X) == 0 || (start.Y - end.Y) == 0) &&
			CheckValidPoint(start) && CheckValidPoint(end)) {
			int h = (end.X - start.X) > 0 ? 1 : (end.X - start.X) < 0 ? -1 : 0;
			int v = (end.Y - start.Y) > 0 ? 1 : (end.Y - start.Y) < 0 ? -1 : 0;
			int x = start.X; int y = start.Y;
			while(x != end.X || y != end.Y) {
                if (GameInstance.Board[x][y] != 0) {
                    print("HIT ON: " + x + " " + y);
                    return false;
                }
				x += h; y += v;
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
		return (GameInstance.Tiles[point.X][point.Y].GetComponent<Transform>().position + new Vector3(0, 0.75f, 0));
	}

    public void AddAI(int id) {
        ActiveAI.Add(AllAI[id]);
    }
    public void RemoveAI(int id) {
        ActiveAI.Remove(AllAI[id]);
    }

    // TIMEOUT THREADING STUFF

    private void ActivateCurrentPlayer() {
        CurrentMove = CurrentPlayer.AI.YourTurn();
        print(CurrentMove.MoveTo.X);
        print(CurrentMove.MoveTo.Y);
        print(CurrentMove.ShootTo.X);
        print(CurrentMove.ShootTo.Y);

    }
    static void CallWithTimeout(Action action, int timeoutMilliseconds) {
        Thread threadToKill = null;
        Action wrappedAction = () => {
            threadToKill = Thread.CurrentThread;
            action();
        };

        IAsyncResult result = wrappedAction.BeginInvoke(null, null);
        if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds)) {
            wrappedAction.EndInvoke(result);
        }
        else {
            threadToKill.Abort();
            throw new TimeoutException();
        }
    }
}

public class Amazon : Amazons.Pawn {
	public GameObject Pawn;
	public Amazon(GameObject pawn, int id, Point pos) {
		ID = id;
		Pawn = pawn;
        Position = pos;
	}
}

public class GameInst : Amazons.Game {
	public List<List<GameObject>> Tiles;
    public void SetPlayers(Amazons.Player[] players) {
        Players = players;
    }
	public void SetBoard(List<List<GameObject>> tiles, List<List<int>> newBoard, int width, int height) {
		Tiles = tiles;
        Board = newBoard;
        Height = height;
        Width = width;
    }
}
