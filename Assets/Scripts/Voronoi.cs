using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Arch_IA_Piccioni.Voronoi
{
    public sealed partial class Voronoi<IDType> where IDType : IEquatable<IDType>
    {

        private readonly struct HalfPlane
        {
            public readonly Vector2 normal;
            public readonly float d;
            public readonly IDType owner;
            public readonly Vector2 origin;
            public readonly Vector2 direction;

            public HalfPlane(Vector2 rawNormal, float rawD, IDType owner)
            {
                float length = rawNormal.magnitude;
                Vector2 n = rawNormal / length;
                float dist = rawD / length;

                normal = n;
                d = dist;
                this.owner = owner;
                origin = n * dist;
                direction = new Vector2(-n.y, n.x);
            }
        }

        private sealed class Region
        {
            public List<HalfPlane> planes;
            public bool isEmpty;

            public Region()
            {
                planes = new List<HalfPlane>();
            }

            public Region(Region other)
            {
                planes = new List<HalfPlane>(other.planes);
                isEmpty = other.isEmpty;
            }

            public bool Add(HalfPlane plane)
            {
                planes.Add(plane);

                List<HalfPlane> kept = new List<HalfPlane>(planes.Count);

                for (int i = 0; i < planes.Count; i++)
                {
                    if (ComputeInterval(planes, i, out _, out _))
                        kept.Add(planes[i]);
                }

                isEmpty = kept.Count == 0;
                planes = kept;

                return !isEmpty;
            }

            public bool TryGetMostViolated(Vector2 position, out HalfPlane worst)
            {
                worst = default;
                float worstViolation = Mathf.Epsilon;
                bool violated = false;

                for (int i = 0; i < planes.Count; i++)
                {
                    HalfPlane plane = planes[i];
                    float violation = Vector2.Dot(plane.normal, position) - plane.d;

                    if (violation > worstViolation)
                    {
                        worstViolation = violation;
                        worst = plane;
                        violated = true;
                    }
                }

                return violated;
            }

            private static bool ComputeInterval(List<HalfPlane> halfPlanes, int planeIndex, out float lowerBound, out float upperBound)
            {
                HalfPlane currentPlane = halfPlanes[planeIndex];

                lowerBound = float.NegativeInfinity;
                upperBound = float.PositiveInfinity;

                for (int otherPlaneIndex = 0; otherPlaneIndex < halfPlanes.Count; otherPlaneIndex++)
                {
                    if (otherPlaneIndex == planeIndex)
                        continue;

                    HalfPlane otherPlane = halfPlanes[otherPlaneIndex];

                    float constraintAtOrigin = Vector2.Dot(otherPlane.normal, currentPlane.origin) - otherPlane.d;
                    float directionConstraint = Vector2.Dot(otherPlane.normal, currentPlane.direction);

                    if (Mathf.Abs(directionConstraint) < Mathf.Epsilon)
                    {
                        if (constraintAtOrigin > Mathf.Epsilon)
                            return false;

                        continue;
                    }

                    float intersectionParameter = -constraintAtOrigin / directionConstraint;

                    if (directionConstraint > 0f)
                        upperBound = Mathf.Min(upperBound, intersectionParameter);
                    else
                        lowerBound = Mathf.Max(lowerBound, intersectionParameter);

                    if (upperBound - lowerBound <= Mathf.Epsilon)
                        return false;
                }

                return true;
            }
        }

        private sealed class Cell
        {
            public IDType id;
            public Vector2 position;
            public float weight;
            public Region region = new Region();
            public HashSet<IDType> neighbors = new HashSet<IDType>();
            public bool IsEmpty => region.isEmpty;
        }

        private IDType defaultID = default(IDType);
        public IDType DefaultValue => defaultID;

        private readonly Dictionary<IDType, Cell> cells = new Dictionary<IDType, Cell>();
        private readonly Dictionary<Type, List<IDType>> objectIDsByType = new Dictionary<Type, List<IDType>>();
        private readonly HashSet<IDType> emptyCells = new HashSet<IDType>();

        public Voronoi(IDType defaultID)
        {
            SetDefaultID(defaultID);
        }

        public Voronoi()
        {
        }

        public void SetDefaultID(IDType defaultID)
        {
            this.defaultID = defaultID;
        }

        public IDType GetNearestObject(Vector3 position)
        {
            return TryGetNearestObject(position, out IDType best) ? best : defaultID;
        }

        public bool TryGetNearestObject(Vector3 position, out IDType best)
        {
            best = defaultID;

            Cell current = GetStartCell();

            if (current == null)
            {
                Debug.LogWarning("Start cell is null");
                return false;
            }

            for (int step = 0; step <= cells.Count; step++)
            {
                if (!current.region.TryGetMostViolated(position, out HalfPlane worst))
                {
                    best = current.id;
                    return true;
                }

                if (!cells.TryGetValue(worst.owner, out Cell next) || next.IsEmpty)
                    return false;

                current = next;
            }

            return false;
        }

        private Cell GetStartCell()
        {
            foreach (Cell c in cells.Values)
            {
                if (!c.IsEmpty)
                    return c;
            }

            return null;
        }

        public void Add(IDType id, Vector2 position, float weight = 0.0f)
        {
            if (cells.ContainsKey(id))
            {
                UpdateObject(id, position, weight);
                return;
            }

            Cell cell = new Cell { id = id, position = position, weight = weight };

            cells[id] = cell;

            RecomputeCell(cell);
            RecomputeAll(CellsOverlapping(cell));
        }

        public void UpdateObject(IDType id, Vector2 newPosition)
        {
            if (!cells.TryGetValue(id, out Cell cell))
                return;

            UpdateObject(id, newPosition, cell.weight);
        }

        public void UpdateObject(IDType id, Vector2 newPosition, float weight)
        {
            if (!cells.TryGetValue(id, out Cell cell))
                return;

            HashSet<IDType> affected = new HashSet<IDType>(cell.neighbors);
            affected.UnionWith(emptyCells);

            cell.position = newPosition;
            cell.weight = weight;

            RecomputeCell(cell);

            affected.UnionWith(CellsOverlapping(cell));
            affected.Remove(id);

            RecomputeAll(affected);
        }

        public void Remove(IDType id)
        {
            if (!cells.TryGetValue(id, out Cell removed))
                return;

            HashSet<IDType> affected = new HashSet<IDType>(removed.neighbors);
            affected.UnionWith(emptyCells);
            affected.Remove(id);

            cells.Remove(id);
            emptyCells.Remove(id);

            foreach (List<IDType> idsOfType in objectIDsByType.Values)
                idsOfType.Remove(id);

            RecomputeAll(affected);
        }

        private void RecomputeAll(IEnumerable<IDType> ids)
        {
            List<IDType> idList = new List<IDType>(ids);

            for (int i = 0; i < idList.Count; i++)
            {
                if (cells.TryGetValue(idList[i], out Cell c))
                    RecomputeCell(c);
            }
        }

        private void RecomputeCell(Cell cell)
        {
            List<Cell> others = new List<Cell>(cells.Count);

            foreach (Cell other in cells.Values)
            {
                if (!other.id.Equals(cell.id))
                    others.Add(other);
            }

            Vector2 origin = cell.position;

            others.Sort((a, b) =>
                (a.position - origin).sqrMagnitude.CompareTo((b.position - origin).sqrMagnitude));

            Region region = new Region();

            foreach (Cell other in others)
            {
                Vector2 normal = other.position - cell.position;

                if (normal.sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon)
                    continue;

                float d = Vector2.Dot(normal, (cell.position + other.position) * 0.5f)
                          + (cell.weight - other.weight) * 0.5f;

                if (!region.Add(new HalfPlane(normal, d, other.id)))
                    break;
            }

            cell.region = region;
            cell.neighbors.Clear();

            if (region.isEmpty)
            {
                emptyCells.Add(cell.id);
                return;
            }

            emptyCells.Remove(cell.id);

            foreach (HalfPlane plane in region.planes)
                cell.neighbors.Add(plane.owner);
        }

        private static bool Overlaps(Region a, Region b)
        {
            if (a.isEmpty || b.isEmpty)
                return false;

            Region merged = new Region(a);

            foreach (HalfPlane plane in b.planes)
            {
                if (!merged.Add(plane))
                    return false;
            }

            return true;
        }

        private HashSet<IDType> CellsOverlapping(Cell cell)
        {
            HashSet<IDType> result = new HashSet<IDType>();

            if (cell.IsEmpty)
                return result;

            foreach (Cell other in cells.Values)
            {
                if (!other.id.Equals(cell.id) && Overlaps(cell.region, other.region))
                    result.Add(other.id);
            }

            return result;
        }

    }
}