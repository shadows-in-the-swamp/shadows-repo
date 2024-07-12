using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public abstract class Sense : MonoBehaviour
{
    [SerializeField] protected LayerMask _producerLayers;
    [SerializeField] protected PerceptionMark _perceptionMarkPrefab;
    protected Character _character;
    protected Dictionary<int,PerceptionMark> _marks = new();
    public event Action<PerceptionMark> OnFirstSense;
    public event Action<PerceptionMark> OnSense;    
    public List<PerceptionMark> Marks
    {
        get
        {
            return _marks.Values.ToList();
        }
    }

    protected virtual void Awake()
    {
        _character = GetComponentInParent<Character>();
        if (OnFirstSense == null)
        {
            OnFirstSense = ActionsUtils.Noop1;
        }
        if (OnSense == null)
        {
            OnSense = ActionsUtils.Noop1;
        }
    }
    protected virtual void LateUpdate()
    {
        CleanMarks();   
    }

    protected virtual void CleanMarks()
    {
        var marksKeys = _marks.Keys.ToArray();
        foreach (var key in marksKeys)
        {
            PerceptionMark mark = _marks[key];
            if (!mark.gameObject.activeSelf)
            {
                _marks.Remove(key);
                Destroy(mark.gameObject);
            }
        }
    }

    protected virtual void SetPerceptionMarkFrom(GameObject producer, Transform origin)
    {
        int producerID = producer.GetInstanceID();
        if (producerID == _character.gameObject.GetInstanceID() || !_producerLayers.Includes(producer.layer))
        {
            return;
        }
        _marks.TryGetValue(producerID, out PerceptionMark mark);
        if (mark != null)
        {
            if (!mark.gameObject.activeSelf)
            {
                _marks.Remove(producerID);
                Destroy(mark.gameObject);
            }
            else
            {
                mark.ResetTime();
                mark.RefreshPosition(producer, origin);
                OnSense(mark);
            }
        }
        else
        {
            mark = Instantiate(_perceptionMarkPrefab, origin.position , _perceptionMarkPrefab.transform.rotation);
            _marks.Add(producerID, mark);
            mark.Initialize(producer);
            mark.RefreshPosition(producer, origin);
            OnFirstSense(mark);
        }
    }

}
