using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UTestArena
{
    public Vector2 Min = new Vector2(-300, -300);
    public Vector2 Max = new Vector2(300, 300);

    public Vector3 NewRandomPoint(float magnitude = 1.0f)
    {
        return new Vector3(
            Random.Range(Min.x, Max.x) * magnitude,
            0,
            Random.Range(Min.y, Max.y) * magnitude);
    }

    public Rect Bound { get { return new Rect(Min.x, Min.y, Max.x - Min.x, Max.y - Min.y); } }
}

public class UTestPrototypes
{
    public static List<GameObject> Prototypes = new List<GameObject>();

    public static void Init()
    {
        GameObject go = GameObject.Find("Prototypes");
        if (go != null)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                GameObject prototype = go.transform.GetChild(i).gameObject;
                if (prototype != null)
                {
                    Prototypes.Add(prototype);
                }
            }

            Debug.Log(go.transform.childCount.ToString() + " prototypes found.");
        }
    }

    public static GameObject NewRandom()
    {
        int i = Random.Range(0, Prototypes.Count - 1);
        return Object.Instantiate(Prototypes[i]) as GameObject;
    }
}

public class UQuadtreeTest : MonoBehaviour
{
    UTestArena m_arena = new UTestArena();
    GameObject m_instRoot = null;
    GameObject m_player = null;
    GameObject m_moveTarget = null;

    UQuadtree m_qt;

    bool _alwaysMove = false;
    bool _drawDebugLines = false;

    void Start()
    {
        UTestPrototypes.Init();

        m_player = GameObject.Find("Player");

        Renderer r = m_player.GetComponent<Renderer>() as Renderer;
        Bounds b = r.bounds;

        m_moveTarget = GameObject.Find("MoveTarget");

        m_instRoot = GameObject.Find("Instances");
        if (m_instRoot != null)
        {
            for (int i = 0; i < 5000; i++)
            {
                GameObject instance = UTestPrototypes.NewRandom();
                instance.transform.localPosition = m_arena.NewRandomPoint();
                instance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                instance.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                instance.transform.parent = m_instRoot.transform;
            }
        }

        m_qt = new UQuadtree(m_arena.Bound);
    }

    void Update()
    {
        if (_drawDebugLines)
            UCore.DrawRect(m_arena.Bound, 0.1f, Color.white);

        Vector3 target = m_moveTarget.transform.position;
        Vector3 dist = target - m_player.transform.position;
        if (dist.magnitude > 1.0f)
        {
            Vector3 delta = dist.normalized * Time.deltaTime * 100.0f;
            if (delta.magnitude > dist.magnitude)
            {
                m_player.transform.position = target;
            }
            else
            {
                m_player.transform.position += delta;
            }
        }
        else
        {
            if (_alwaysMove)
            {
                SetNewTarget();
            }
        }

        m_qt.Update(new Vector2(m_player.transform.position.x, m_player.transform.position.z));
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(50, Screen.height * 0.1f - 40, 80, 40), "Move"))
        {
            SetNewTarget();
        }

        _alwaysMove = GUI.Toggle(new Rect(50, Screen.height * 0.2f, 100, 20), _alwaysMove, "Always Move");
        _drawDebugLines = GUI.Toggle(new Rect(50, Screen.height * 0.25f, 100, 20), _drawDebugLines, "Debug Lines");

        if (_drawDebugLines != m_qt.EnableDebugLines)
            m_qt.EnableDebugLines = _drawDebugLines;
    }

    void SetNewTarget()
    {
        m_moveTarget.transform.position = m_arena.NewRandomPoint(0.8f);
    }
}
