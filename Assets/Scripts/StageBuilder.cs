using UnityEngine;
using System.Collections;
using TileType = DungeonGenerator.Stage.TileType;

namespace DungeonGenerator
{
	public abstract class StageBuilder : MonoBehaviour
	{

		public Stage stage;
		//	//Rect get bounds => stage.bounds;
		public Rect bounds;
		//	//void generate(Stage stage);

		//	void bindStage(Stage stage)
		//	{
		//		this.stage = stage;
		//	}

		public TileType getTile(Vec pos) {
			return stage[pos].type;
		}
		public void setTile(Vec pos, TileType type)
		{
			stage[pos].type = type;
		}

		public void fill(TileType tile)
		{
			for (var y = 0; y < stage.height; y++)
			{
				for (var x = 0; x < stage.width; x++)
				{
					setTile(new Vec(x, y), tile);
				}
			}
		}

		//	/// Randomly turns some [wall] tiles into [floor] and vice versa.
		//	void erode(int iterations, { TileType floor, TileType wall}) {
		//    if (floor == null) floor = Tiles.floor;
		//    if (wall == null) wall = Tiles.wall;

		//    final bounds = stage.bounds.inflate(-1);
		//    for (var i = 0; i<iterations; i++) {
		//      // TODO: This way this works is super inefficient. Would be better to
		//      // keep track of the floor tiles near open ones and choose from them.
		//      var pos = rng.vecInRect(bounds);

		//	var here = getTile(pos);
		//      if (here != wall) continue;

		//      // Keep track of how many floors we're adjacent too. We will only erode
		//      // if we are directly next to a floor.
		//      var floors = 0;

		//      for (var dir in Direction.ALL) {
		//        var tile = getTile(pos + dir);
		//        if (tile == floor) floors++;
		//      }

		//      // Prefer to erode tiles near more floor tiles so the erosion isn't too
		//      // spiky.
		//      if (floors< 2) continue;
		//      if (rng.oneIn(9 - floors)) setTile(pos, floor);
		//}
		//  }
	}

	public class Vec
	{
		public int x;
		public int y;

		public Vec(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}
}