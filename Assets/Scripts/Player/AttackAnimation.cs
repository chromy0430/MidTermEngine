using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimation : MonoBehaviour
{
    private Animator anim;

    private bool attacked = false;
    public TrailRenderer trailRenderer;
    
    private void Start()
    {
        TryGetComponent<Animator>(out anim);
        
        trailRenderer.enabled = false;
    }

    private void Update()
    {
        if(!attacked)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                anim.SetTrigger("AttackTrigger");
                attacked = true;
            }
            
            attacked = false;
        }
    }

    public void eventkey()
    {
        trailRenderer.enabled = true;
    }

    public void eventkey2()
    {
        trailRenderer.enabled = false;
    }
}
