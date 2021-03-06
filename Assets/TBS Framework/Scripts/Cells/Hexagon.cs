using System.Collections.Generic;
using UnityEngine;

namespace TbsFramework.Cells
{
    /// <summary>
    /// Implementation of hexagonal cell.
    /// </summary>
    public abstract class Hexagon : Cell
    {
        protected List<Cell> neighbours = null;
        /// <summary>
        /// HexGrids comes in four types regarding the layout. 
        /// This distinction is necessary to convert cube coordinates to offset and vice versa.
        /// </summary>
        [HideInInspector]
        public HexGridType HexGridType;

        /// <summary>
        /// Converts offset coordinates into cube coordinates.
        /// Cube coordinates is another system of coordinates that makes calculation on hex grids easier.
        /// </summary>
        public Vector3 CubeCoord
        {
            get
            {
                Vector3 ret = new Vector3();
                switch (HexGridType)
                {
                    case HexGridType.odd_q:
                        {
                            ret.x = OffsetCoord.x;
                            ret.z = OffsetCoord.y - (OffsetCoord.x + (Mathf.Abs(OffsetCoord.x) % 2)) / 2;
                            ret.y = -ret.x - ret.z;
                            break;
                        }
                    case HexGridType.even_q:
                        {
                            ret.x = OffsetCoord.x;
                            ret.z = OffsetCoord.y - (OffsetCoord.x - (Mathf.Abs(OffsetCoord.x) % 2)) / 2;
                            ret.y = -ret.x - ret.z;
                            break;
                        }
                }
                return ret;
            }
        }

        /// <summary>
        /// Converts cube coordinates back to offset coordinates.
        /// </summary>
        /// <param name="cubeCoords">Cube coordinates to convert.</param>
        /// <returns>Offset coordinates corresponding to given cube coordinates.</returns>
        protected Vector2 CubeToOffsetCoords(Vector3 cubeCoords)
        {
            Vector2 ret = new Vector2();

            switch (HexGridType)
            {
                case HexGridType.odd_q:
                    {
                        ret.x = cubeCoords.x;
                        ret.y = cubeCoords.z + (cubeCoords.x + (Mathf.Abs(cubeCoords.x) % 2)) / 2;
                        break;
                    }
                case HexGridType.even_q:
                    {
                        ret.x = cubeCoords.x;
                        ret.y = cubeCoords.z + (cubeCoords.x - (Mathf.Abs(cubeCoords.x) % 2)) / 2;
                        break;
                    }
            }
            return ret;
        }

        protected static readonly Vector3[] _directions =  {
        new Vector3(+1, -1, 0), new Vector3(+1, 0, -1), new Vector3(0, +1, -1),
        new Vector3(-1, +1, 0), new Vector3(-1, 0, +1), new Vector3(0, -1, +1)};

        public override int GetDistance(Cell other)
        {
            var _other = other as Hexagon;
            int distance = (int)(Mathf.Abs(CubeCoord.x - _other.CubeCoord.x) + Mathf.Abs(CubeCoord.y - _other.CubeCoord.y) + Mathf.Abs(CubeCoord.z - _other.CubeCoord.z)) / 2;
            Debug.Log("Distance is: " + distance.ToString());
            return distance;
        }//Distance is given using Manhattan Norm.

        public override List<Cell> GetNeighbours(List<Cell> cells)
        {
            if (neighbours == null)
            {
                neighbours = new List<Cell>(6);
                foreach (var direction in _directions)
                {
                    var neighbour = cells.Find(c => c.OffsetCoord == CubeToOffsetCoords(CubeCoord + direction));
                    if (neighbour == null) continue;
                    neighbours.Add(neighbour);
                }
            }
            return neighbours;

        }//Each hex cell has six neighbors, which positions on grid relative to the cell are stored in _directions constant.

        public override void CopyFields(Cell newCell)
        {
            newCell.OffsetCoord = OffsetCoord;
            (newCell as Hexagon).HexGridType = HexGridType;
        }
    }

    public enum HexGridType
    {
        even_q,
        odd_q,
        even_r,
        odd_r
    };
}