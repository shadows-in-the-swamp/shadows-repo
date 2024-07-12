using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Collider))]
public class Eyes : Sense
{
    [SerializeField] protected LayerMask _sightLayers;
    protected readonly List<IActionZone> _actionZones = new();
    protected  List<SightZone> _sightZones = new();

    public virtual List<IActionZone> Actions
    {
        get
        {
            return new List<IActionZone>(_actionZones);
        }
    }
    public virtual bool HasActions
    {
        get
        {
            return _actionZones.Count > 0;
        }
    }
    public virtual bool HasSight
    {
        get 
        {
            var marks = _marks.Values.ToArray();
            foreach (PerceptionMark mark in marks)
            {
                if (mark.gameObject.activeSelf)
                {
                    return true;
                }
            }
            return false;
        }
    }


    protected virtual void FixedUpdate()
    {
        SightFixedUpdate();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        CleanActions();
    }

    protected virtual void CleanActions()
    {
        var actionZones = Actions;
        foreach (var action in actionZones)
        {
            var actionObject = action as MonoBehaviour;
            if (actionObject.IsDestroyed())
            {
                _actionZones.Remove(action);
            }
        }
    }

    protected virtual void SightFixedUpdate()
    {
        foreach (SightZone zone in _sightZones)
        {
            Vector3 direction = zone.transform.position - transform.position;
            if (Physics.Raycast(new Ray(transform.position, direction), out RaycastHit hit, float.PositiveInfinity, _sightLayers))
            {
                if (hit.collider.gameObject.TryGetComponent(out SightZone sightZone))
                {
                    OnSight(sightZone);
                }
            }
        }
    }

    protected virtual void OnSight(SightZone sightZone)
    {
        SetPerceptionMarkFrom(sightZone.From, sightZone.transform);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SightZone sightZone))
        {
            if (!_sightZones.Contains(sightZone))
            {
                _sightZones.Add(sightZone);
            }
        }
        if (other.gameObject.TryGetComponent(out IActionZone actionZone))
        {
            if (!_actionZones.Contains(actionZone))
            {
                _actionZones.Add(actionZone);
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SightZone sightZone))
        {
            if (_sightZones.Contains(sightZone))
            {
                _sightZones.Remove(sightZone);
            }
        }
        if (other.gameObject.TryGetComponent(out IActionZone actionZone))
        {
            if (_actionZones.Contains(actionZone))
            {
                _actionZones.Remove(actionZone);
            }
        }
    }

    protected virtual void OnDrawGizmos()
    {
        foreach (SightZone zone in _sightZones)
        {
            Vector3 direction = zone.transform.position - transform.position;
            Gizmos.DrawLine(transform.position, zone.transform.position);
            if (Physics.Raycast(new Ray(transform.position, direction), out RaycastHit hit, float.PositiveInfinity, _sightLayers))
            {
                if (hit.collider.gameObject.TryGetComponent(out SightZone sightZone))
                {
                    if (_producerLayers.Includes(sightZone.From.layer))
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(transform.position, hit.transform.position);
                    }
                }
            }
        }
    }
}
