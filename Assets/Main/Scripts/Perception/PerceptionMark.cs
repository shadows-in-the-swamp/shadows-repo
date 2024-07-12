using UnityEngine;

public class PerceptionMark : MonoBehaviour
{
    [SerializeField] protected float _duration = 10f;
    protected float _time = 0f;
    protected bool _paused = false;

    protected virtual void Update()
    {
        if (_paused)
        {
            return;
        }
        if (_time < _duration)
        {
            _time += Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public virtual void ResetTime()
    {
        _time = 0f;
    }

    public virtual void Pause()
    {
        _paused = true;
    }

    public virtual void Resume()
    {
        _paused = false;
    }

    public virtual void RefreshPosition(GameObject origin, Transform point)
    {
        transform.position = point.position;
    }

    public virtual void Initialize(GameObject origin)
    {
        // Does nothing by default
    }
}
