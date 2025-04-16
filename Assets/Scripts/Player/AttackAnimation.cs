using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimation : MonoBehaviour
{
    private Animator anim;

    private bool attacked = false;
    public TrailRenderer trailRenderer;
    public ParticleSystem particleSystem;
    private ParticleSystem.TrailModule trailModule; // TrailModule 추가
    
    private void Start()
    {
        TryGetComponent<Animator>(out anim);
        
        if (trailRenderer is null) return;
        
        trailRenderer.enabled = false;

        if (particleSystem is null) return;
        particleSystem.Stop();
        
        // TrailModule 초기화 및 비활성화
        trailModule = particleSystem.trails;
        trailModule.enabled = false;
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
        particleSystem.Play();
        trailModule.enabled = true; // Trail 활성화
    }

    public void eventkey2()
    {
        trailRenderer.enabled = false;
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // 파티클 정지 및 제거
        trailModule.enabled = false; // Trail 비활성화
    }
}
