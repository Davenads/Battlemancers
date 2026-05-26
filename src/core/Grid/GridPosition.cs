namespace Battlemancers.Core.Grid
{
    /// <summary>
    /// Represents a discrete position on the game grid.
    /// Pure C# — no Unity dependency. Use this in all simulation layer code.
    /// This replaces UnityEngine.Vector2Int in the simulation layer to keep it fully decoupled.
    /// </summary>
    public struct GridPosition : System.IEquatable<GridPosition>
    {
        /// <summary>The horizontal axis coordinate (column).</summary>
        public int X { get; }

        /// <summary>The vertical axis coordinate (row).</summary>
        public int Y { get; }

        /// <summary>Initializes a new GridPosition with the given x and y coordinates.</summary>
        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        // --- Static helpers ---

        /// <summary>The origin position (0, 0).</summary>
        public static readonly GridPosition Zero = new GridPosition(0, 0);

        /// <summary>One step in the positive Y direction (north).</summary>
        public static readonly GridPosition Up = new GridPosition(0, 1);

        /// <summary>One step in the negative Y direction (south).</summary>
        public static readonly GridPosition Down = new GridPosition(0, -1);

        /// <summary>One step in the negative X direction (west).</summary>
        public static readonly GridPosition Left = new GridPosition(-1, 0);

        /// <summary>One step in the positive X direction (east).</summary>
        public static readonly GridPosition Right = new GridPosition(1, 0);

        // --- Distance and adjacency ---

        /// <summary>
        /// Returns the Manhattan distance to another GridPosition.
        /// Manhattan distance is the sum of absolute differences in X and Y.
        /// Used for range calculations throughout the simulation.
        /// </summary>
        public int ManhattanDistance(GridPosition other)
        {
            return System.Math.Abs(X - other.X) + System.Math.Abs(Y - other.Y);
        }

        /// <summary>
        /// Returns true if the other position is directly adjacent (4-directional: N, S, E, W only).
        /// Diagonal positions are NOT considered adjacent by this method.
        /// </summary>
        public bool IsAdjacentTo(GridPosition other)
        {
            return ManhattanDistance(other) == 1;
        }

        /// <summary>
        /// Returns true if the other position is adjacent including diagonals (8-directional).
        /// Both X and Y differences must be at most 1, and the position must not be this position itself.
        /// </summary>
        public bool IsAdjacentOrDiagonalTo(GridPosition other)
        {
            int dx = System.Math.Abs(X - other.X);
            int dy = System.Math.Abs(Y - other.Y);
            return dx <= 1 && dy <= 1 && (dx + dy) > 0;
        }

        // --- Operator overloads ---

        /// <summary>Adds two GridPositions component-wise.</summary>
        public static GridPosition operator +(GridPosition a, GridPosition b)
        {
            return new GridPosition(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>Subtracts b from a component-wise.</summary>
        public static GridPosition operator -(GridPosition a, GridPosition b)
        {
            return new GridPosition(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>Returns true if both X and Y are equal.</summary>
        public static bool operator ==(GridPosition a, GridPosition b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        /// <summary>Returns true if X or Y differ.</summary>
        public static bool operator !=(GridPosition a, GridPosition b)
        {
            return !(a == b);
        }

        // --- IEquatable<GridPosition> ---

        /// <inheritdoc/>
        public bool Equals(GridPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is GridPosition other)
                return Equals(other);
            return false;
        }

        /// <summary>
        /// Hash combines X and Y for use in dictionaries and hash sets.
        /// Uses a prime-multiplier approach to minimize collisions on small grids.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
