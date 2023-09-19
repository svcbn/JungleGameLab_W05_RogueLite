using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Node : MonoBehaviour, IPointerClickHandler
{
    public int step;
    public bool isActive { get; set; } = false;
    public Node parentNode;
    Image image;
    float branchAngle;

    void Start()
    {
        if(step == 0)
        {
            isActive = true;
        }
    }

    public void Initialize(Vector3 direction, float length)
    {
        if (step <= TreeGenerator.Instance.depth)
        {
            DrawLine(this.transform.position, parentNode.transform.position);
            branchAngle = CaculateNodeAngle();
            Vector2[] directions = RotateAngle(direction, branchAngle);

            for (int i = 0; i < TreeGenerator.Instance.width; i++)
            {
                image = GetComponent<Image>();

                var go = Instantiate(TreeGenerator.Instance.node);
                go.transform.position = directions[i].normalized * length + (Vector2)this.transform.position;
                
                TreeGenerator.Instance.nodes.Add(this);

                Node _node = go.GetComponent<Node>();
                _node.step = this.step + 1;
                _node.parentNode = this;
                _node.transform.SetParent(_node.parentNode.transform);

                Vector3 newDir = ((Vector2)go.transform.position - (Vector2)transform.position).normalized;
                _node.Initialize(newDir, length);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        ActiveCheck();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!parentNode.isActive) return;
        isActive = !isActive;
        if (isActive)
        {
            ActiveNode();
        }
        else
        {
            InActiveNode();
        }
    }

    void DrawLine(Vector3 from, Vector3 to)
    {
        TreeGenerator.Instance.DrawLine(from, to);
    }

    void ActiveCheck()
    {
        if(parentNode == null) return;
        if (!parentNode.isActive)
        {
            InActiveNode() ;
        }
    }

    void ActiveNode()
    {
        isActive = true;
        image.color = Color.red;
    }

    void InActiveNode()
    {
        isActive = false;
        image.color = Color.white;
    }

    float CaculateNodeAngle()
    {
        float branchCount = TreeGenerator.Instance.points * Mathf.Pow( TreeGenerator.Instance.width, step - 1);
        return 360 / TreeGenerator.Instance.points / branchCount * 1.28f;
    }

    Vector2[] RotateAngle(Vector2 direction, float degree)
    {
        Vector2[] angles = new Vector2[TreeGenerator.Instance.width];

        switch (TreeGenerator.Instance.width)
        {
            case 2:
                angles[0] = Quaternion.AngleAxis(-degree, Vector3.forward) * direction;

                angles[1] = Quaternion.AngleAxis(degree, Vector3.forward) * direction;
                break;
            case 3:
                angles[0] = Quaternion.AngleAxis(-degree, Vector3.forward) * direction;

                angles[1] = direction;

                angles[2] = Quaternion.AngleAxis(degree, Vector3.forward) * direction;
                break;
            case 4:
                angles[0] = Quaternion.AngleAxis(-degree * 2, Vector3.forward) * direction;

                angles[1] = Quaternion.AngleAxis(-degree, Vector3.forward) * direction;

                angles[2] = Quaternion.AngleAxis(degree, Vector3.forward) * direction;

                angles[3] = Quaternion.AngleAxis(degree * 2, Vector3.forward) * direction;
                break;
        }

        return angles;
    }
}
