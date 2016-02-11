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

		public void BindStage(Stage stage)
		{
			this.stage = stage;
		}

		public TileType GetTile(Vec pos)
		{
			return stage[pos].type;
		}
		public void SetTile(Vec pos, TileType type)
		{
			//Debug.Log("set tile: " + stage[pos]);
			stage[pos].type = type;
		}

		public void Fill(TileType tile)
		{
			for (int y = 0; y < stage.height; y++)
			{
				for (int x = 0; x < stage.width; x++)
				{
					SetTile(new Vec(x, y), tile);
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

		public int Distance(Vec other)
		{
			return (int)(Mathf.Sqrt(Mathf.Pow(other.x - x, 2) + Mathf.Pow(other.y - y, 2)));
		}

		public static Vec operator *(Vec vec, float multiplier)
		{
			return new Vec((int)(vec.x * multiplier), (int)(vec.y * multiplier));
		}

		public static Vec operator +(Vec vec1, Vec vec2)
		{
			return new Vec(vec1.x + vec2.x, vec1.y + vec2.y);
		}

		public static Vec operator -(Vec vec1, Vec vec2)
		{
			return new Vec(vec1.x - vec2.x, vec1.y - vec2.y);
		}

		public Vector2 ToVector2()
		{
			return new Vector2(x, y);
		}
	}
}