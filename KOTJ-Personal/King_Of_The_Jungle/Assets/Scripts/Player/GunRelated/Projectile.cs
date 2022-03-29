using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage; //Damage passed from Weapon Class
    public float lifeTimeSeconds;
    public float splashRange;
    public AudioSource soundExplosion;
    private PhotonView PV;
    public LayerMask LayerToHit;

    //private float force = 2000;
    public GameObject explosion;




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (PV.IsMine)
        {
            if (splashRange > 0)
            {
                explode();
            }
            else
            {
                //Damage sent when object instantiated
                collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage);
            }
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        Destroy(gameObject, lifeTimeSeconds);
    }

    void explode()
    {
        var hitColliders = Physics2D.OverlapCircleAll(transform.position, splashRange, LayerToHit);
        foreach (var hitCollider in hitColliders)
        {
            var enemy = hitCollider.GetComponent<PlayerController>();
            if (enemy)
            {
                var closestPoint = hitCollider.ClosestPoint(transform.position);
                var distance = Vector2.Distance(closestPoint, transform.position);
                var damagePercent = Mathf.InverseLerp(splashRange, 0, distance);

                //Vector2 direction = enemy.transform.position - transform.position;
                //hitCollider.gameObject.GetComponent<Rigidbody2D>().AddForce(direction * force);

                hitCollider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage * damagePercent);
            }

            ExplosionEffect();
            
        }
    }

    public void ExplosionEffect()
    {
        PV.RPC("RPC_ExplosionEffect", RpcTarget.All);
    }

    [PunRPC]
    void RPC_ExplosionEffect()
    {
        GameObject ExplosionEffect = Instantiate(explosion, transform.position, Quaternion.identity);
        soundExplosion.Play(0);
        Destroy(ExplosionEffect, 0.35f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, splashRange);
    }

}
