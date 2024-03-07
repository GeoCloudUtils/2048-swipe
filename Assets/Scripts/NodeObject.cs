using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class NodeObject : MonoBehaviour
{
    [NonSerialized] public Node from = null;
    [NonSerialized] public Node target = null;

    public bool combine = false;

    private int m_value;

    public int Value
    {
        get => m_value;
        set
        {
            this.m_value = value;
            this.valueText.text = value.ToString();
            SetColor(value);
        }
    }

    public Image blockImage;
    public TextMeshProUGUI valueText;

    private void SetColor(int value)
    {
        Color color = new Color(0.776f, 0.714f, 0.667f); // Default color

        switch (value)
        {
            case 2:
            case 4:
                color = new Color(0.929f, 0.886f, 0.855f); // Light gray
                break;
            case 8:
            case 16:
                color = new Color(0.929f, 0.875f, 0.773f); // Light brown
                break;
            case 32:
            case 64:
                color = new Color(0.957f, 0.678f, 0.478f); // Light orange
                break;
            case 128:
            case 256:
                color = new Color(0.957f, 0.573f, 0.412f); // Light orange
                break;
            case 512:
            case 1024:
                color = new Color(0.957f, 0.427f, 0.216f); // Light orange
                break;
            case 2048:
            case 4096:
                color = new Color(0.957f, 0.290f, 0.290f); // Light red
                break;
        }
        blockImage.color = color;
    }


    public void InitializeFirstValue()
    {
        int[] v = new int[] { 2, 4 };
        this.Value = v[Random.Range(0, v.Length)];
    }
    public void MoveToNode(Node from, Node to)
    {
        combine = false;
        this.from = from;
        this.target = to;
    }

    public void CombineToNode(Node from, Node to)
    {
        combine = true;
        this.from = from;
        this.target = to;
    }
    public void OnEndMove()
    {
        if (target != null)
        {
            if (combine)
            {
                target.realNodeObj.Value = Value * 2;
                var t = target.realNodeObj.transform.DOPunchScale(new Vector3(.25f, .25f, .25f), 0.15f, 3);
                this.gameObject.SetActive(false);
                t.onComplete += () =>
                {
                    this.needDestroy = true;
                    this.target = null;
                    this.from = null;
                };
            }
            else
            {
                this.from = null;
                this.target = null;
            }
        }
    }
    public bool needDestroy = false;

    public void SetAngle(float angle)
    {
        int snappedAngle = Mathf.RoundToInt(angle / 90) * 90;
        float z = 0;
        if (snappedAngle == 90)
        {
            z = -90;
        }
        else if (snappedAngle == 180)
        {
            z = 180;
        }
        else if (snappedAngle == 270)
        {
            z = 90;
        }
        Quaternion rotation = Quaternion.Euler(0, 0, z);
        transform.rotation = rotation;
    }

    public void StartMoveAnimation()
    {
        if (target != null)
        {
            this.name = target.point.ToString();
            var tween = this.blockImage.rectTransform.DOLocalMove(target.position, 0.1f);
            tween.onComplete += () =>
            {
                OnEndMove();
            };
        }

    }
    public void UpdateMoveAnimation()
    {
        if (target != null)
        {
            this.name = target.point.ToString();
            var p = Vector2.Lerp(this.transform.localPosition, target.position, 0.35f);
            this.transform.localPosition = p;
            if (Vector2.Distance(this.transform.localPosition, target.position) < 0.25f)
            {
                OnEndMove();
            }
        }
    }
}
