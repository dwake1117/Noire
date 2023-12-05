using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlesAttract : MonoBehaviour
{
    private ParticleSystem m_System;
    private Transform follow;
    ParticleSystem.Particle[] m_Particles;
    private const float baseSpeed = 12;
    private const float speedMultiplier = 4f;
    private const float EPS_distance = 0.5f;
    
    private float speed;
    private bool init;
    
    /// initializes a particle attraction system following Transform `t` with `num` number of particles  
    public void Init(int num, Transform t)
    {
        if (m_System == null)
            m_System = GetComponent<ParticleSystem>();

        if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
            m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
        
        follow = t;
        speed = baseSpeed;
        
        // sets a single burst with `num` particles
        var em = m_System.emission;
        em.enabled = true;
        em.rateOverTime = 0;
        em.SetBursts(
            new ParticleSystem.Burst[]{
                new (0, num),
            });
        
        m_System.Play();
        StartCoroutine(InitWithDelay());
    }

    private IEnumerator InitWithDelay()
    {
        yield return new WaitForSeconds(1f);
        init = true;
    }

    private void Update()
    {
        if (init)
        {
            int numParticlesAlive = m_System.GetParticles(m_Particles);
            if (numParticlesAlive == 0)
            {
                Destroy(gameObject);
            }
            
            float step = speed * Time.deltaTime;
            speed += Time.deltaTime * speedMultiplier;
            
            for (int i = 0; i < numParticlesAlive; i++)
            {
                if (Vector3.Distance(m_Particles[i].position, follow.position) < EPS_distance) 
                    m_Particles[i].remainingLifetime = .2f;
                else
                    m_Particles[i].position = Vector3.Slerp(m_Particles[i].position, follow.position, step);
            }

            m_System.SetParticles(m_Particles, numParticlesAlive);
        }
    }
}