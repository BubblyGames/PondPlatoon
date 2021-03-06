using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//[System.Serializable]
public class Path
{
    public int Length
    {
        get
        {
            if (cells != null)
                return cells.Length;
            else
                return 0;
        }
    }

    const int MAX_SEGMENT_LENGTH = 10;

    public int id = -1;
    public bool dirty = true;
    public bool initiated = false;
    public Vector3Int start = new Vector3Int();
    public Vector3Int end = new Vector3Int();


    CubeWorldGenerator world;

    internal CellInfo[] cells = new CellInfo[0];
    public List<Midpoint> midPoints = new List<Midpoint>();
    List<Midpoint> midpointsCopy = new List<Midpoint>();
    List<EnemyBehaviour> enemies = new List<EnemyBehaviour>();

    List<Node> closedList;

    Node result;

    int currentStep = 1;

    public Path(CubeWorldGenerator world)
    {
        this.world = world;
    }

    public void Prepare()
    {
        //When this function is called, it's important that the midpoints have already been added

        currentStep = 1;

        closedList = new List<Node>();

        //Adjust midpoints so they are valid
        foreach (Midpoint m in midPoints)
        {
            //Check if it's floating
            if (world.CheckIfFloating(m.cell))
            {
                //Debug.Log("Floating");
                CellInfo c;
                c = world.GetCellUnderWithGravity(m.cell);
                if (c == null)
                {
                    //Debug.Log("fffffffffffff");
                    continue;
                }
                c.normalInt = m.cell.normalInt;
                m.cell = c;
            }

            //Check if it's not walkable
            if (!m.cell.canWalk)
            {
                CellInfo c;
                c = world.GetClosestWalkableCell(m.cell);
                c.normalInt = m.cell.normalInt;//?????????
                m.cell = c;
            }
        }

        start = midPoints[0].cell.GetPosInt();
        end = midPoints[midPoints.Count() - 1].cell.GetPosInt();

        midpointsCopy = new List<Midpoint>(midPoints);

        result = new Node(world.GetCell(start));
        result.isMidpoint = true;
        result.ComputeFScore(end.x, end.y, end.z);
    }

    public bool FindPath()
    {
        Prepare();

        while (HasNextStep())
        {
            if (!GoToNextMidpoint())
            {

            }
        }

        SavePath();

        return true;
    }

    public bool SavePath()
    {
        if (result == null || result.Position != end)
        {
            midPoints = new List<Midpoint>(midpointsCopy);
            return false;
        }

        //midPoints = midpointsCopy;

        result.normal = Vector3Int.up;
        lastCell = world.GetCellUnder(result.cell);

        //List of cells in the path
        List<CellInfo> pathCells = new List<CellInfo>();
        List<Midpoint> newMidpoints = new List<Midpoint>();
        //midPoints.Clear();

        midPoints.Clear();
        AddMidpoint(midpointsCopy[0]);

        int count = 0;
        while (result != null)
        {
            if (result.isFloating)
            {
                result = result.Parent;
                continue;
            }

            CellInfo cell = world.cells[result.x, result.y, result.z];
            cell.normalInt = GetNormalOf(cell);
            cell.isPath = true;

            pathCells.Add(cell);
            cell.paths.Add(this);

            if (result.midpoint != null)
            {
                newMidpoints.Add(result.midpoint);
                count = 0;
            }
            if (count > MAX_SEGMENT_LENGTH && !cell.endZone)
            {
                newMidpoints.Add(new Midpoint(result.cell, false));
                count = 0;
            }

            result = result.Parent;
            count++;
        }

        for (int j = 0; j < Mathf.Min(pathCells.Count, 7); j++)
        {
            pathCells[j].isPath = false;
        }

        //Cells are added from last to first, so we reverse the list
        pathCells.Reverse();

        //New midpoints are reversed and saved
        newMidpoints.Reverse();
        foreach (Midpoint m in newMidpoints)
        {
            AddMidpoint(m);
        }
        //midPoints = newMidpoints;

        //Send cells to path to store it
        cells = pathCells.ToArray();

        dirty = false;
        initiated = true;

        return true;
    }

    public bool HasNextStep()
    {
        return currentStep <= midPoints.Count - 1;
    }

    bool previousSuccess = true;
    public bool GoToNextMidpoint()
    {
        bool lastSept = currentStep == midPoints.Count - 1 || currentStep == 1; //Is this the segment bewteen the last midpoint and the end?

        Midpoint midpoint = midPoints[currentStep];
        Node current = Path.FindPathAstar(world, result, midpoint.cell, lastSept, world.canMergePaths && previousSuccess, closedList);//

        if (current == null)
        {
            //current = Path.FindPathAstar(world, result, midpoint.cell, lastSept, world.canMergePaths, closedList);//
            //result = null;
            previousSuccess = false;
            currentStep++;
            return false;
        }

        Node n = current;
        n.isMidpoint = true;
        n.midpoint = midpoint;

        while (n.Parent != null && n.Parent != result)
        {
            closedList.Add(n);
            n.cell.isPath = true;
            foreach (CellInfo c in world.GetNeighbours(n.cell, true))
            {
                c.isCloseToPath = true;
            }
            n = n.Parent;
        }

        //The end of this segment will become the start of the next one
        currentStep++;
        result = current;
        previousSuccess = true;
        return true;
    }

    public static Node FindPathAstar(CubeWorldGenerator _world, Node firstNode, CellInfo end,
        bool lastStep, bool canMergePaths, List<Node> excludedNodes = null)
    {
        float startTime = Time.realtimeSinceStartup;

        Node current;

        NodeComparer nodeComparer = new NodeComparer();
        //SortedSet<Node> openList = new SortedSet<Node>(nodeComparer);
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        /* if (excludedNodes != null)
             closedList.AddRange(excludedNodes);*/

        //SortedSet<Node> sortedList = new SortedSet<Node>();

        //First node, with starting position and null parent
        firstNode.ComputeFScore(end.x, end.y, end.z);
        current = firstNode;
        openList.Add(firstNode);

        int count = 0;
        while (openList.Count > 0 && count < 1000)
        {
            count++;
            //Sorting the list in "h" in increasing order
            openList.Sort(nodeComparer);

            //Check lists's first node
            //current = openList.Min;

            current = openList[0];
            closedList.Add(current);
            openList.Remove(current);

            if (current.cell == end)//If first node is goal,returns current Node3D
            {
                //Debug.Log("Got there in " + (Time.realtimeSinceStartup - startTime) + "s");
                return current;
            }
            else
            {
                //Expands neightbors, (compute cost of each one) and add them to the list
                CellInfo[] neighbours = _world.GetNeighbours(current.cell);

                current.isFloating = true;
                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (!neighbours[i].canWalk)
                    {
                        current.isFloating = false;
                        break;
                    }
                }

                if (current.isFloating && current.Parent != null && current.Parent.isFloating)
                    continue;

                foreach (CellInfo neighbour in neighbours)
                {
                    if (neighbour == null ||
                        !neighbour.canWalk ||
                        (!lastStep && neighbour.endZone) ||
                        (!canMergePaths && neighbour.isPath && current.cell.isPath))//||(neighbour.isPath && !neighbour.endZone)
                        continue;

                    //if neighbour is in open or closed lists go to next neighbour
                    if (openList.Any(node => node.cell == neighbour) || closedList.Any(node => node.cell == neighbour))
                        continue;

                    Node n = new Node(neighbour);

                    n.cell = _world.cells[n.x, n.y, n.z];
                    n.Parent = current;
                    n.ComputeFScore(end.x, end.y, end.z);

                    openList.Add(n);
                }
            }
        }

        Debug.LogWarning("Couldn't get to: " + end.GetPos());
        return null;
    }

    #region Midpoints
    public bool AddMidpoint(Midpoint midpoint)
    {
        if (midPoints.Contains(midpoint) || midpoint.cell == null)// midPoints.Any(mid => mid.cell == midpoint.cell)
        {
            //Debug.Log("Fuck you");
            return false;
        }
        midPoints.Add(midpoint);
        return true;
    }

    public bool InsertMidpoint(int i, Midpoint midpoint)
    {
        if (midPoints.Contains(midpoint))
        {
            return false;
        }
        midPoints.Insert(i, midpoint);
        return true;
    }

    #endregion

    public Vector3 GetStep(int idx) { return new Vector3(cells[idx].x, cells[idx].y, cells[idx].z); }

    internal CellInfo GetCell(int idx)
    {
        return cells[idx];
    }

    public void AddEnemy(EnemyBehaviour enemy)
    {
        enemies.Add(enemy);
    }

    public void Empty()
    {
        foreach (EnemyBehaviour e in enemies)
        {
            e.FindNewPath();
        }

        foreach (CellInfo c in cells)
        {
            c.RemovePath(this);
        }

        dirty = true;

        enemies.Clear();
    }

    public void Reset()
    {
        cells = null;
        midPoints.Clear();
        midpointsCopy.Clear();
        dirty = true;
        initiated = false;
    }

    CellInfo lastCell;
    Vector3Int GetNormalOf(CellInfo c)
    {
        if (c.endZone)
        {
            if (c.y > world.size / 2)
            {
                lastCell = world.GetCell(c.GetPosInt() + Vector3Int.down);
                return Vector3Int.up;
            }
            else
            {
                lastCell = world.GetCell(c.GetPosInt() + Vector3Int.up);
                return Vector3Int.down;
            }
        }

        Vector3Int result;
        CellInfo[] neighbours = world.GetNeighbours(c);
        CellInfo[] _neigbours;

        foreach (CellInfo n in neighbours)
        {
            if (n.blockType == BlockType.Air)
                continue;

            if (n == lastCell && n != c)
            {
                result = c.GetPosInt() - n.GetPosInt();
                lastCell = n;
                //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = lastCell.GetPos();
                return result;
            }

            _neigbours = world.GetNeighbours(n);

            foreach (CellInfo _n in _neigbours)
            {
                if (_n.blockType != BlockType.Air && _n == lastCell && _n != c)
                {
                    result = c.GetPosInt() - n.GetPosInt();
                    lastCell = n;
                    //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = lastCell.GetPos();
                    return result;
                }
            }
        }

        result = Vector3ToIntNormalized(c.GetPos() - lastCell.GetPos());


        neighbours = world.GetNeighbours(c, true);

        int best = -1;
        float minDist = Mathf.Infinity;

        //Sometimes all neighbours are air and can't find the best option
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i].blockType != BlockType.Air)
            {
                float dist = Vector3.Distance(c.GetPos(), neighbours[i].GetPos());
                if (dist < minDist)
                {
                    best = i;
                    minDist = dist;
                }
            }
        }

        if (best == -1)
        {
            best = UnityEngine.Random.Range(0, neighbours.Length - 1);
        }

        lastCell = neighbours[best];

        return result;
    }


    Vector3Int GetNormalOff(CellInfo c)
    {
        if (lastCell == null)
        {
            if (c.y > world.size / 2)
            {
                lastCell = world.GetCell(c.GetPosInt() + Vector3Int.down);
                return Vector3Int.up;
            }
            else
            {
                lastCell = world.GetCell(c.GetPosInt() + Vector3Int.up);
                return Vector3Int.down;
            }
        }

        Vector3 dir = c.GetPos() - lastCell.GetPos();

        CellInfo[] neighbours = world.GetNeighbours(c);
        foreach (CellInfo n in neighbours)
        {
            Vector3 dir2 = c.GetPos() - n.GetPos();
            if (!n.canWalk && Vector3.Dot(dir, dir2) > 0)
            {
                lastCell = n;
                return Vector3Int.RoundToInt(1.5f * dir2);
            }
        }

        return Vector3Int.zero;
    }

    public static Vector3Int Vector3ToIntNormalized(Vector3 dir)
    {
        Vector3Int dirInt = new Vector3Int();
        if (dir.x > 0)
            dirInt.x = Mathf.RoundToInt(dir.x + 0.49f);
        else
            dirInt.x = Mathf.RoundToInt(dir.x - 0.49f);

        if (dir.y > 0)
            dirInt.y = Mathf.RoundToInt(dir.y + 0.49f);
        else
            dirInt.y = Mathf.RoundToInt(dir.y - 0.49f);

        if (dir.z > 0)
            dirInt.z = Mathf.RoundToInt(dir.z + 0.49f);
        else
            dirInt.z = Mathf.RoundToInt(dir.z - 0.49f);

        return dirInt;
    }
}