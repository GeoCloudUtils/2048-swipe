using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public enum State
    {
        WAIT, PROCESSING, END
    }

    public State state = State.WAIT;
    public static Board Instance
    {
        get
        {
            if (_inst == null)
            {
                _inst = FindObjectOfType<Board>();
            }
            return _inst;
        }
    }

    private static Board _inst;

    public List<NodeObject> realNodeList = new List<NodeObject>();
    public List<Node> nodeData = new List<Node>();

    public Dictionary<Vector2Int, Node> nodeMap = new Dictionary<Vector2Int, Node>();

    public int col = 4;
    public int row = 4;

    public GameObject emptyNodePrefab;
    public GameObject nodePrefab;

    public InputManager inputManager;

    public RectTransform emptyNodeRect;
    public RectTransform realNodeRect;

    public event Action<int> OnCombine;
    public event Action OnGameOverAction;

    private bool isRotating;

    public void OnGameOver()
    {
        Debug.Log("Game Over!!!!");
        OnGameOverAction?.Invoke();
    }

    private void RotatePanel(float angle)
    {
        isRotating = true;
        Vector3 currentRotation = this.transform.eulerAngles;
        Vector3 newRotation = new Vector3(currentRotation.x, currentRotation.y, Mathf.RoundToInt(currentRotation.z + angle));
        this.transform.DORotate(newRotation, .5f).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            isRotating = false;
            if (newRotation.z == 0 || newRotation.z == 360f)
            {
                MoveTo(Node.Direction.DOWN);
            }
            else if (newRotation.z == 90f)
            {
                MoveTo(Node.Direction.LEFT);
            }
            else if (newRotation.z == 180f)
            {
                MoveTo(Node.Direction.UP);
            }
            else
            {
                MoveTo(Node.Direction.RIGHT);
            }
            foreach (NodeObject data in realNodeList)
            {
                data.SetAngle(transform.rotation.z);
            }
        });
    }
    private void CreateBoard()
    {
        realNodeList.Clear();
        nodeMap.Clear();
        nodeData.Clear();

        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                var instantiatePrefab = GameObject.Instantiate(emptyNodePrefab, emptyNodeRect.transform, false);
                var point = new Vector2Int(j, i);
                Vector2Int left = point - new Vector2Int(1, 0);
                Vector2Int down = point - new Vector2Int(0, 1);
                Vector2Int right = point + new Vector2Int(1, 0);
                Vector2Int up = point + new Vector2Int(0, 1);
                Vector2Int?[] v = new Vector2Int?[4];

                if (IsValid(right))
                {
                    v[0] = right;
                }

                if (IsValid(down))
                {
                    v[1] = down;
                }

                if (IsValid(left))
                {
                    v[2] = left;
                }

                if (IsValid(up))
                {
                    v[3] = up;
                }

                Node node = new Node(v)
                {
                    point = point,
                    nodeRectObj = instantiatePrefab
                };
                nodeData.Add(node);
                instantiatePrefab.name = node.point.ToString();
                this.nodeMap.Add(point, node);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(emptyNodeRect);
        foreach (var data in nodeData)
        {
            data.position = data.nodeRectObj.GetComponent<RectTransform>().localPosition;
        }
    }

    private bool IsValid(Vector2Int point)
    {
        if (point.x == -1 || point.x == row || point.y == col || point.y == -1)
        {
            return false;
        }
        return true;
    }
    private void CreateBlock(int x, int y)
    {
        if (nodeMap[new Vector2Int(x, y)].realNodeObj != null)
        {
            return;
        }

        GameObject realNodeObj = Instantiate(nodePrefab, realNodeRect.transform, false);
        var node = nodeMap[new Vector2Int(x, y)];
        var pos = node.position;
        realNodeObj.GetComponent<RectTransform>().localPosition = pos;
        realNodeObj.transform.DOPunchScale(new Vector3(.32f, .32f, .32f), 0.15f, 3);
        var nodeObj = realNodeObj.GetComponent<NodeObject>();
        this.realNodeList.Add(nodeObj);
        nodeObj.InitializeFirstValue();
        node.value = nodeObj.Value;
        node.realNodeObj = nodeObj;
        node.realNodeObj.SetAngle(transform.rotation.z);
        //Vector3 currentRotation = this.transform.eulerAngles;
        //Vector3 newRotation = new Vector3(currentRotation.x, currentRotation.y, Mathf.RoundToInt(currentRotation.z));
        //if (newRotation.z == 0 || newRotation.z == 360f)
        //{
        //    MoveTo(Node.Direction.DOWN);
        //}
        //else if (newRotation.z == 90f)
        //{
        //    MoveTo(Node.Direction.LEFT);
        //}
        //else if (newRotation.z == 180f)
        //{
        //    MoveTo(Node.Direction.UP);
        //}
        //else
        //{
        //    MoveTo(Node.Direction.RIGHT);
        //}
    }

    public void Combine(Node from, Node to)
    {
        to.value *= 2;
        from.value = null;
        if (from.realNodeObj != null)
        {
            from.realNodeObj.CombineToNode(from, to);
            from.realNodeObj = null;
            to.combined = true;
        }
        OnCombine?.Invoke((int)to.value);
    }

    public void Move(Node from, Node to)
    {
        to.value = from.value;
        from.value = null;
        if (from.realNodeObj != null)
        {
            from.realNodeObj.MoveToNode(from, to);
            if (from.realNodeObj != null)
            {
                to.realNodeObj = from.realNodeObj;
                from.realNodeObj = null;
            }
        }
    }

    public void MoveTo(Node.Direction dir)
    {
        if (dir == Node.Direction.RIGHT)
        {
            for (int j = 0; j < col; j++)
            {
                for (int i = (row - 2); i >= 0; i--)
                {
                    var node = nodeMap[new Vector2Int(i, j)];
                    if (node.value == null)
                    {
                        continue;
                    }
                    var right = node.FindTarget(node, Node.Direction.RIGHT);
                    if (right != null)
                    {
                        if (node.value.HasValue && right.value.HasValue)
                        {
                            if (node.value == right.value)
                            {
                                Combine(node, right);
                            }
                        }
                        else if (right != null && right.value.HasValue == false)
                        {
                            Move(node, right);
                        }
                        else if (right == null)
                        {
                            return;
                        }
                    }
                }
            }

        }
        if (dir == Node.Direction.LEFT)
        {
            for (int j = 0; j < col; j++)
            {
                for (int i = 1; i < row; i++)
                {
                    var node = nodeMap[new Vector2Int(i, j)];
                    if (node.value == null)
                    {
                        continue;
                    }

                    var left = node.FindTarget(node, Node.Direction.LEFT);
                    if (left != null)
                    {
                        if (node.value.HasValue && left.value.HasValue)
                        {
                            if (node.value == left.value)
                            {
                                Combine(node, left);
                            }
                        }
                        else if (left != null && left.value.HasValue == false)
                        {
                            Move(node, left);
                        }
                    }
                }
            }

        }
        if (dir == Node.Direction.UP)
        {
            for (int j = col - 2; j >= 0; j--)
            {
                for (int i = 0; i < row; i++)
                {
                    var node = nodeMap[new Vector2Int(i, j)];
                    if (node.value == null)
                    {
                        continue;
                    }
                    var up = node.FindTarget(node, Node.Direction.UP);
                    if (up != null)
                    {
                        if (node.value.HasValue && up.value.HasValue)
                        {
                            if (node.value == up.value)
                            {
                                Combine(node, up);
                            }
                        }
                        else if (up != null && up.value.HasValue == false)
                        {
                            Move(node, up);
                        }
                    }
                }
            }
        }
        if (dir == Node.Direction.DOWN)
        {
            for (int j = 1; j < col; j++)
            {
                for (int i = 0; i < row; i++)
                {
                    var node = nodeMap[new Vector2Int(i, j)];
                    if (node.value == null)
                    {
                        continue;
                    }
                    var down = node.FindTarget(node, Node.Direction.DOWN);
                    if (down != null)
                    {
                        if (node.value.HasValue && down.value.HasValue)
                        {
                            if (node.value == down.value)
                            {
                                Combine(node, down);
                            }
                        }
                        else if (down != null && down.value.HasValue == false)
                        {
                            Move(node, down);
                        }
                    }
                }
            }
        }

        foreach (NodeObject data in realNodeList)
        {
            if (data.target != null)
            {
                state = State.PROCESSING;
                data.StartMoveAnimation();
            }
        }

        Show();

        if (IsGameOver())
        {
            OnGameOver();
        }
    }

    public bool IsGameOver()
    {
        bool gameOver = true;
        nodeData.ForEach(x =>
        {
            for (int i = 0; i < x.linkedNode.Length; i++)
            {
                if (x.realNodeObj == null)
                {
                    gameOver = false;
                }

                if (x.linkedNode[i] == null)
                {
                    continue;
                }

                var nearNode = nodeMap[x.linkedNode[i].Value];
                if (x.value != null && nearNode.value != null && x.value == nearNode.value)
                {
                    gameOver = false;
                }
            }
        });

        return gameOver;
    }
    private void CreateRandom()
    {
        var emptys = nodeData.FindAll(x => x.realNodeObj == null);
        if (emptys.Count == 0)
        {
            if (IsGameOver())
            {
                OnGameOver(); ;
            }
        }
        else
        {
            var rand = UnityEngine.Random.Range(0, emptys.Count);
            var pt = emptys[rand].point;
            CreateBlock(pt.x, pt.y);
        }
    }
    public void Awake()
    {
        inputManager.OnSwipe += OnSwipe;
        CreateBoard();
        MoveTo(Node.Direction.DOWN);
    }

    private void OnSwipe(InputManager.Direction direction)
    {
        if (isRotating || state != State.WAIT)
        {
            return;
        }
        float angle = direction == InputManager.Direction.Left ? -90f : 90f;
        RotatePanel(angle);
    }

    public void UpdateState()
    {
        bool targetAllNull = true;
        foreach (var data in realNodeList)
        {
            if (data.target != null)
            {
                targetAllNull = false;
                break;
            }
        }

        if (targetAllNull)
        {
            if (state == State.PROCESSING)
            {
                var removed = new List<NodeObject>();
                List<NodeObject> removeTarget = new List<NodeObject>();
                foreach (var data in realNodeList)
                {
                    if (data.needDestroy)
                    {
                        removeTarget.Add(data);
                    }
                }

                removeTarget.ForEach(x =>
                {
                    realNodeList.Remove(x);
                    GameObject.Destroy(x.gameObject);
                });
                state = State.END;
            }
        }

        if (state == State.END)
        {
            nodeData.ForEach(x => x.combined = false);
            state = State.WAIT;
            CreateRandom();
        }
    }

    private void Show()
    {
        string v = null;
        for (int i = col - 1; i >= 0; i--)
        {
            for (int j = 0; j < row; j++)
            {
                var p = nodeMap[new Vector2Int(j, i)].value;
                string t = p.ToString();
                if (p.HasValue == false)
                {
                    t = "N";
                }
                if (p == 0)
                {
                    t = "0";
                }
                v += t + " ";
            }
            v += "\n";
        }
        Debug.Log(v);
    }
    public void Update()
    {
        if (isRotating)
        {
            return;
        }
        UpdateState();
        if (state == State.WAIT)
        {
            if (Input.GetKeyUp(KeyCode.RightArrow)) MoveTo(Node.Direction.RIGHT);
            if (Input.GetKeyUp(KeyCode.LeftArrow)) MoveTo(Node.Direction.LEFT);
            if (Input.GetKeyUp(KeyCode.UpArrow)) MoveTo(Node.Direction.UP);
            if (Input.GetKeyUp(KeyCode.DownArrow)) MoveTo(Node.Direction.DOWN);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Show();
        }
    }

    public void Start()
    {
        CreateRandom();
    }
}


