
using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ExorcismWeakness : Weakness<Exorcism>
{
    [SerializeField] protected float _maxDuration = 5f;
    protected ParticleSystem _particles;
    protected float _duration = 0f;
    public override string Hint
    {
        get
        {
            return $"{base.Hint} ({_maxDuration - _duration})";
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _particles = GetComponent<ParticleSystem>();
    }

    protected override void Start()
    {
        base.Start();
        if (_active)
        {
            _particles.Play();
        }
    }

    public override void Activate()
    {
        base.Activate();
        _particles.Play();
    }

    public override void Deactivate()
    {
        base.Deactivate();
        _particles.Stop();
    }

    public override void ActionatedBy(Player player, Action<string> Callback = null)
    {
        player.CastSpell<Exorcism>(Callback);
    }

    public override bool BeeingActionatedBy(Player player)
    {
        bool beeingActionated = _duration < _maxDuration;
        if (beeingActionated)
        {
            _duration += Time.deltaTime;
            return beeingActionated;
        }
        else
        {
            _enemy.Exorcised();
            _duration = 0;
            return false;
        }
    }

}
