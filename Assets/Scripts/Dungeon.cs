using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using Tiles = DungeonGenerator.Stage.Tile;
using Vec = DungeonGenerator.Vec;
using System.Linq;

namespace DungeonGenerator
{
	public class Dungeon : StageBuilder
	{

		//	library hauberk.content.maze_dungeon;

		//	import 'dart:math' as math;

		//import 'package:piecemeal/piecemeal.dart';

		//import '../engine.dart';
		//import 'stage_builder.dart';
		//import 'tiles.dart';

		///// The random dungeon generator.
		/////
		///// Starting with a stage of solid walls, it works like so:
		/////
		///// 1. Place a number of randomly sized and positioned rooms. If a room
		/////    overlaps an existing room, it is discarded. Any remaining rooms are
		/////    carved out.
		///// 2. Any remaining solid areas are filled in with mazes. The maze generator
		/////    will grow and fill in even odd-shaped areas, but will not touch any
		/////    rooms.
		///// 3. The result of the previous two steps is a series of unconnected rooms
		/////    and mazes. We walk the stage and find every tile that can be a
		/////    "connector". This is a solid tile that is adjacent to two unconnected
		/////    regions.
		///// 4. We randomly choose connectors and open them or place a door there until
		/////    all of the unconnected regions have been joined. There is also a slight
		/////    chance to carve a connector between two already-joined regions, so that
		/////    the dungeon isn't single connected.
		///// 5. The mazes will have a lot of dead ends. Finally, we remove those by
		/////    repeatedly filling in any open tile that's closed on three sides. When
		/////    this is done, every corridor in a maze actually leads somewhere.
		/////
		///// The end result of this is a multiply-connected dungeon with rooms and lots
		///// of winding corridors.
		//abstract class Dungeon extends StageBuilder
		//	{
		public GameObject wall;
		public GameObject floor;
		public Vector2 tileSize = Vector2.one;

		public int numRoomTries;

		//  /// The inverse chance of adding a connector between two regions that have
		//  /// already been joined. Increasing this leads to more loosely connected
		//  /// dungeons.
		[Range(0,1)]
		public float extraConnectorChance = 0.2f;

		//  /// Increasing this allows rooms to be larger.
		public int roomExtraSize = 0;
		[Range(0,1)]
		public float windingPercent = 0f;

		List<Rect> _rooms = new List<Rect>();

		//	/// For each open position in the dungeon, the index of the connected region
		//	/// that that position is a part of.
		int[,] _regions;

		//	/// The index of the current region being carved.
		int _currentRegion = -1;
		private System.Random rng;
		public string seed = "0";

		private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

		public bool makeRooms = true;
		public bool makeMaze = true;
		public bool connectStuff = true;
		public bool clearStuff = true;
		public bool doDecorate = true;
		public bool showHistory = false;
		public float cameraAdjust = 0.5f;
		int printedTimes = 0;

		void Start()
		{
			sw.Start();
			Debug.Log("Timer started: " + sw.Elapsed.ToString());
			Tiles.Initialize();
			stage = new Stage((int)bounds.width, (int)bounds.height);
			Generate(stage);
		}

		void Generate(Stage stage)
		{
			if (stage.width % 2 == 0 || stage.height % 2 == 0)
			{
				throw new System.ArgumentException("The stage must be odd-sized.");
			}
			rng = new System.Random(seed.GetHashCode());
			BindStage(stage);

			Fill(Tiles.wall);
			_regions = new int[stage.width, stage.height];
			//for (int y = 0; y < stage.height; y++)
			//{
			//    for (int x = 0; x < stage.width; x++)
			//    {
			//        _regions[x, y] = -1;
			//    }
			//}
			Debug.Log("Map initialized: " + sw.Elapsed.ToString());


			if (makeRooms)
			{
				AddRooms();
				Debug.Log("Rooms made: " + sw.Elapsed.ToString());
				if (showHistory)
				{
					PrintOutput();
				}
			}

			// Fill in all of the empty space with mazes.
			if (makeMaze)
			{
				for (int y = 1; y < bounds.height; y += 2)
				{
					for (int x = 1; x < bounds.width; x += 2)
					{
						Vec pos = new Vec(x, y);
						if (GetTile(pos) != Tiles.wall) continue;
						GrowMaze(pos);
					}
				}

				Debug.Log("Maze made: " + sw.Elapsed.ToString());
				if (showHistory)
				{
					PrintOutput();
				}
			}

			if (connectStuff)
			{
				ConnectRegions();
				Debug.Log("Stuff connected: " + sw.Elapsed.ToString());
				if (showHistory)
				{
					PrintOutput();
				}
			}
			if (clearStuff)
			{
				RemoveDeadEnds();
				Debug.Log("Stuff cleared: " + sw.Elapsed.ToString());
				if (showHistory)
				{
					PrintOutput();
				}
			}

			if (doDecorate)
			{
				foreach (Rect room in _rooms)
				{
					OnDecorateRoom(room);
				}
				Debug.Log("Decorated: " + sw.Elapsed.ToString());

				if (showHistory)
				{
					PrintOutput();
				}
			}
			Debug.Log("Random stuff done: " + sw.Elapsed.ToString());
			if (!showHistory)
			{
				PrintOutput();
			}
			sw.Stop();
		}
		List<GameObject> outputGOs = new List<GameObject>();
		void PrintOutput()
		{
			//foreach (GameObject go in outputGOs)
			//{
			//	Destroy(go);
			//}
			//outputGOs.Clear();
			//string output = "";
			for (int y = 0; y < stage.tiles.GetLength(1); y++)
			{
				for (int x = 0; x < stage.tiles.GetLength(0); x++)
				{
					//output += stage.tiles[x, y].type == Tiles.wall ? 0 : 1;
					outputGOs.Add(Instantiate(stage.tiles[x, y].type == Tiles.wall ? wall : floor,
						new Vector3(tileSize.x * x + printedTimes * (tileSize.x * stage.tiles.GetLength(0) + 3), tileSize.y * y),
						Quaternion.identity) as GameObject);
				}
				//output += Environment.NewLine;
			}
			printedTimes++;
			//print("output:");
			//print(output);
			Debug.Log("Stuff printed: " + sw.Elapsed.ToString());
			AdjustCamera();
		}

		void AdjustCamera()
		{
			Camera camera = FindObjectOfType<Camera>();
			camera.transform.position = new Vector3((printedTimes * (tileSize.x * stage.tiles.GetLength(0) + 3)) / 2, stage.tiles.GetLength(1) * tileSize.y / 2, camera.transform.position.z);
			camera.orthographicSize = Mathf.Max(printedTimes * bounds.width, bounds.height) * cameraAdjust;
		}

		void OnDecorateRoom(Rect room) { }

		/// Implementation of the "growing tree" algorithm from here:
		/// http://www.astrolog.org/labyrnth/algrithm.htm.
		void GrowMaze(Vec start)
		{
			List<Vec> cells = new List<Vec>();
			Vec lastDir = Direction.NONE;

			StartRegion();
			Carve(start, Tiles.floor);

			cells.Add(start);
			while (cells.Count > 0)
			{
				Vec cell = cells[cells.Count - 1];

				// See which adjacent cells are open.
				List<Vec> unmadeCells = new List<Vec>();

				foreach (Vec dir in Direction.CARDINAL)
				{
					if (CanCarve(cell, dir)) unmadeCells.Add(dir);
				}

				if (unmadeCells.Count > 0)
				{
					// Based on how "windy" passages are, try to prefer carving in the
					// same direction.
					Vec dir;
					if (unmadeCells.Contains(lastDir) && rng.NextDouble() > windingPercent)
					{
						dir = lastDir;
					}
					else
					{
						dir = unmadeCells[rng.Next(unmadeCells.Count)];
					}

					Carve(cell + dir, Tiles.floor);
					Carve(cell + dir * 2, Tiles.floor);

					cells.Add(cell + dir * 2);
					lastDir = dir;
				}
				else
				{
					// No adjacent uncarved cells.
					cells.Remove(cells.Last());

					// This path has ended.
					lastDir = Direction.NONE;
				}
			}
		}

		/// Places rooms ignoring the existing maze corridors.
		void AddRooms()
		{
			int numberOfTries = 0;
			while(_rooms.Count < numRoomTries)
			//for (int i = 0; i < numRoomTries; i++)
			{
				numberOfTries++;
				// Pick a random room size. The funny math here does two things:
				// - It makes sure rooms are odd-sized to line up with maze.
				// - It avoids creating rooms that are too rectangular: too tall and
				//   narrow or too wide and flat.
				// TODO: This isn't very flexible or tunable. Do something better here.
				int size = rng.Next(1, 3 + roomExtraSize) * 2 + 1;
				int rectangularity = rng.Next(0, 1 + size / 2) * 2;
				int width = size;
				int height = size;
				if (rng.NextDouble() > 0.5f)
				{
					width += rectangularity;
				}
				else
				{
					height += rectangularity;
				}

				int x = rng.Next(((int)(bounds.width - width)) / 2) * 2 + 1;
				int y = rng.Next(((int)(bounds.height - height)) / 2) * 2 + 1;

				Rect room = new Rect(x, y, width, height);

				bool overlaps = false;
				foreach (Rect other in _rooms)
				{
					if (room.Overlaps(other))
					{
						overlaps = true;
						break;
					}
				}

				if (overlaps) continue;

				_rooms.Add(room);

				StartRegion();

				for (int s = x; s < x + width; s++)
				{
					for (int t = y; t < y + height; t++)
					{
						Carve(new Vec(s, t), Tiles.floor);
					}
				}
			}
			print("number of tries: " + numberOfTries);
		}

		void ConnectRegions()
		{
			// Find all of the tiles that can connect two (or more) regions.
			Dictionary<Vec, List<int>> connectorRegions = new Dictionary<Vec, List<int>>();
			for (int y = 1; y < stage.tiles.GetLength(1) - 1; y++)
			{
				for (int x = 1; x < stage.tiles.GetLength(0) - 1; x++)
				{
					Vec pos = new Vec(x, y);
					if (GetTile(pos) != Tiles.wall)
					{
						continue;
					}
					List<int> regions = new List<int>();
					foreach (Vec dir in Direction.CARDINAL)
					{
						Vec posdir = pos + dir;
						if (GetTile(posdir) == Tiles.wall)
						{
							continue;
						}
						int region = _regions[posdir.x, posdir.y];
						if (region >= 0 && !regions.Contains(region))
						{
							regions.Add(region);
						}
					}
					if (regions.Count < 2)
					{
						continue;
					}
					if (!connectorRegions.ContainsKey(pos))
					{
						connectorRegions.Add(pos, regions);
					}
				}
			}
			List<Vec> connectors = connectorRegions.Keys.ToList();

			// Keep track of which regions have been merged. This maps an original
			// region index to the one it has been merged to.

			Dictionary<int, int> merged = new Dictionary<int, int>();
			List<int> openRegions = new List<int>();

			for (int i = 0; i <= _currentRegion; i++)
			{
				merged.Add(i, i);
				openRegions.Add(i);
			}

			// Keep connecting regions until we're down to one.
			while (openRegions.Count > 1)
			{
				//Debug.Log("connectors count" + connectors.Count);
				if (connectors.Count < 1)
				{
					Debug.Log("out of connectors");
					break;
				}
				Vec connector = connectors[rng.Next(connectors.Count)];

				// Carve the connection.
				AddJunction(connector);

				// Merge the connected regions. We'll pick one region (arbitrarily) and
				// map all of the other regions to its index.

				List<int> regions = new List<int>();
				//regions.Add(connectorRegions[connector].Where(region => merged[region]));

				foreach (int region in connectorRegions[connector])
				{
					if (!regions.Contains(merged[region]))
					{
						regions.Add(merged[region]);
					}
				}

				int dest = regions.First();
				List<int> sources = regions.Skip(1).ToList();

				// Merge all of the affected regions. We have to look at *all* of the
				// regions because other regions may have previously been merged with
				// some of the ones we're merging now.
				for (int i = 0; i <= _currentRegion; i++)
				{
					if (sources.Contains(merged[i]))
					{
						merged[i] = dest;
					}
				}

				/*// The sources are no longer in use.
				openRegions.RemoveAll(x =>
				{
					foreach (int source in sources)
					{
						if (x.Equals(source))
						{
							return true;
						}
					}
					return false;
				}
				);*/
				for (int i = 0; i < openRegions.Count; i++)
				{
					foreach (int source in sources)
					{
						if (openRegions[i] == (source))
						{
							openRegions.RemoveAt(i);
						}
					}
				}
				/*// Remove any connectors that aren't needed anymore.
				connectors.RemoveAll(pos =>
				{
					// Don't allow connectors right next to each other.
					if (connector.Distance(pos) < 2) return true;
				
					// If the connector no long spans different regions, we don't need it.
					List<int> _regions = new List<int>();
					foreach (int _region in connectorRegions[connector])
					{
						if (!_regions.Contains(merged[_region]))
						{
							_regions.Add(merged[_region]);
							//_regions = _regions.Distinct().ToList();
						}
					}
				
					if (_regions.Count > 1) return false;
				
					// This connecter isn't needed, but connect it occasionally so that the
					// dungeon isn't singly-connected.
					if (rng.Next(100) < extraConnectorChance) AddJunction(pos);
				
					return true;
				});*/
				for (int i = 0; i < connectors.Count; i++)
				{
					Vec pos = connectors[i];
					if (connector.Distance(pos) < 2)
					{
						connectors.Remove(pos);
					}
					List<int> _regions = new List<int>();
					foreach (int _region in connectorRegions[connector])
					{
						if (!_regions.Contains(merged[_region]))
						{
							_regions.Add(merged[_region]);
						}
					}
					if (_regions.Count > 1)
					{
						//Keep the connector
						continue;
					}
					if (rng.NextDouble() < extraConnectorChance)
					{
						AddJunction(pos);
						connectors.Remove(pos);
					}
				}
			}
		}

		void AddJunction(Vec pos)
		{
			if (rng.NextDouble() > .25f)
			{
				SetTile(pos, rng.NextDouble() > .66f ? Tiles.openDoor : Tiles.floor);
			}
			else
			{
				SetTile(pos, Tiles.closedDoor);
			}
		}

		void RemoveDeadEnds()
		{
			bool done = false;

			while (!done)
			{
				done = true;

				//foreach (var pos in bounds.inflate(-1))
				for (int y = 1; y < bounds.height - 1; y++)
				{
					for (int x = 1; x < bounds.width - 1; x++)
					{
						Vec pos = new Vec(x, y);
						if (GetTile(pos) == Tiles.wall) continue;

						// If it only has one exit, it's a dead end.
						int exits = 0;
						foreach (Vec dir in Direction.CARDINAL)
						{
							if (GetTile(pos + dir) != Tiles.wall) exits++;
						}

						if (exits != 1) continue;

						done = false;
						SetTile(pos, Tiles.wall);
					}
				}
			}
		}

		//	/// Gets whether or not an opening can be carved from the given starting
		//	/// [Cell] at [pos] to the adjacent Cell facing [direction]. Returns `true`
		//	/// if the starting Cell is in bounds and the destination Cell is filled
		//	/// (or out of bounds).</returns>
		bool CanCarve(Vec pos, Vec direction)

		{
			// Must end in bounds.
			if (!bounds.Contains((pos + direction * 3).ToVector2())) return false;

			// Destination must not be open.
			return GetTile(pos + direction * 2) == Tiles.wall;
		}

		public void StartRegion()
		{
			_currentRegion++;
		}

		void Carve(Vec pos, Stage.TileType type)
		{
			if (type == null) type = Tiles.floor;
			SetTile(pos, type);
			_regions[pos.x, pos.y] = _currentRegion;
		}
	}
}

public static class Direction
{
	static public Vec NONE = null;
	static public Vec N = new Vec(0, 1);
	static public Vec NE = new Vec(1, 1);
	static public Vec E = new Vec(1, 0);
	static public Vec SE = new Vec(1, -1);
	static public Vec S = new Vec(0, -1);
	static public Vec SW = new Vec(-1, -1);
	static public Vec W = new Vec(-1, 0);
	static public Vec NW = new Vec(-1, 1);
	//doubles
	static public Vec NN = new Vec(0, 2);
	static public Vec NENE = new Vec(2, 2);
	static public Vec EE = new Vec(2, 0);
	static public Vec SESE = new Vec(2, -2);
	static public Vec SS = new Vec(0, -2);
	static public Vec SWSW = new Vec(-2, -2);
	static public Vec WW = new Vec(-2, 0);
	static public Vec NWNW = new Vec(-2, 2);
	static public Vec[] ALL = new Vec[] { N, NE, E, SE, S, SW, W, NW };
	static public Vec[] DOUBLEALL = new Vec[] { N, NE, E, SE, S, SW, W, NW, NN, NENE, EE, SESE, SS, SWSW, WW, NWNW };
	static public Vec[] CARDINAL = new Vec[] { N, E, S, W };
	static public Vec[] DOUBLECARDINAL = new Vec[] { N, E, S, W, NN, EE, SS, WW };
}