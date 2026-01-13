using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainObject : MonoBehaviour
{
    Collider lastCollider;
    


    protected virtual void OnTriggerEnter(Collider other)  // Can be overridden
    {
        //Debug.Log("TerrainObject Collision Detected");
        lastCollider = other;
        if (other.CompareTag("Player")) {
            ApplyEffect(other.gameObject);
            ApplyEffectEffects();
        }
            
    }

    protected abstract void ApplyEffect(GameObject player);
    protected abstract void ApplyEffectEffects();
}
