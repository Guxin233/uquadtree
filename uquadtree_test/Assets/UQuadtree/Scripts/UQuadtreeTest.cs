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

    bool _alwaysMove = false;

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
                instance.transform.localRotation.SetEulerAngles(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                instance.transform.parent = m_instRoot.transform;
            }
        }
    }

    void Update()
    {
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
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(50, Screen.height * 0.1f - 40, 80, 40), "Move"))
        {
            SetNewTarget();
        }

        _alwaysMove = GUI.Toggle(new Rect(50, Screen.height * 0.2f - 40, 100, 40), _alwaysMove, "Always Move");
    }

    void SetNewTarget()
    {
        m_moveTarget.transform.position = m_arena.NewRandomPoint(0.8f);
    }
}
