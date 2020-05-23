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

    public override void PerformAttack(float damage)
    {
        Projectile _projectile = Instantiate(projectile, ai.hand.position, Quaternion.identity, null);
        _projectile.Initialize(player, gameObject);
        _projectile.projectileHitEvent.AddListener(ProjectileHit);
    }

    public void ProjectileHit()
    {
        //ai.player.makeSound.Invoke(ai.attackVolume);
        base.PerformAttack(attackDamage);
    }

}
