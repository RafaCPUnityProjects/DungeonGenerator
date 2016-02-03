using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileType = DungeonGenerator.Stage.TileType;

namespace DungeonGenerator
{
	public class Tiles
	{
		//		library hauberk.content.tiles;

		//		import 'package:malison/malison.dart';

		//import '../engine.dart';
		//import 'utils.dart';

		///// Static class containing all of the [TileType]s.
		//class Tiles
		//		{
		public static TileType floor;
		public static TileType wall;
		public static TileType lowWall;
		public static TileType table;
		public static TileType openDoor;
		public static TileType closedDoor;
		public static TileType stairs;

		public static TileType grass;
		public static TileType tree;
		public static TileType treeAlt1;
		public static TileType treeAlt2;

		static void initialize()
		{
			// Define the tile types.
			Tiles.floor = new TileType("floor", true, true);
			Tiles.wall = new TileType("wall", false, false);
			Tiles.table = new TileType("table", false, true);
			Tiles.lowWall = new TileType("low wall", false, true);
			Tiles.openDoor = new TileType("open door", true, true);
			Tiles.closedDoor = new TileType("closed door", false, false);
			Tiles.openDoor.closesTo = Tiles.closedDoor;
			Tiles.closedDoor.opensTo = Tiles.openDoor;
			Tiles.stairs = new TileType("stairs", true, true);
			Tiles.grass = new TileType("grass", true, true);
			Tiles.tree = new TileType("tree", false, false);
			Tiles.treeAlt1 = new TileType("tree", false, false);
			Tiles.treeAlt2 = new TileType("tree", false, false);
		}
	}
}
