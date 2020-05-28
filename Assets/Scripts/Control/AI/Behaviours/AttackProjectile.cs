using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackProjectile : Attack
{
    [Header("Projectile--")]
    [SerializeField] Projectile projectile = null;

    private new void OnEnable()
    {
        base.OnEnable();
    }

    public new void Start()
    {
        base.Start();
    }

    public new void Update()
    {
        base.Update();
    }

    public override void PerformAttack(float damage, bool playSound)
    {
        Projectile _projectile = Instantiate(projectile, ai.hand.position, Quaternion.identity, null);
        _projectile.Initialize(player, ai);
        _projectile.projectileHitEvent.AddListener(ProjectileHit);
    }

    public void ProjectileHit(bool playSound)
    {
        base.PerformAttack(attackDamage, playSound);
    }

}
