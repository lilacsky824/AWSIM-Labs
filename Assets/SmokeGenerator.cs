using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof (MeshRenderer))]

[System.Serializable]
public class SmokeGenerator : MonoBehaviour
{
    [Tooltip("Specifies the maximum number of smoke particles in a scene.")]
    [SerializeField]
    [Range(1, 1000)]
    private int maxParticle = 250;
    [Tooltip("Specifies the radius of a circular region where particles are randomly generated in [m].")]
    [SerializeField]
    [Range(0.01f, 3.0f)]
    private float particleRangeRadius = 2.0f;
    [Tooltip("Specifies the size of a smoke particle in [m].")]
    [SerializeField]
    [Range(0.005f, 0.1f)]
    private float particleSize = 0.07f;

    [SerializeField]
    private SmokeParticlePhysics physics;

    // Start is called before the first frame update
    public void Start()
    {
        for (int i = 0; i < maxParticle; i++)
            this.CreateSmokeParticle();
        
        print(GetComponent<MeshRenderer>().material);
    }

    // Update is called once per frame
    public void Update()
    {
        if (this.transform.childCount < maxParticle)
            this.CreateSmokeParticle();
    }

    private void CreateSmokeParticle()
    {
        float angleRad = Random.Range(0.0f, (float)System.Math.PI*2.0f);
        float radius = Random.Range(0.0f, particleRangeRadius);
        SmokeParticle.Create(gameObject, particleSize, radius, angleRad);
    }

    public float GetParticleSize()
    {
        return this.particleSize;
    }

    public float[] GetVelAcc()
    {
        return new float[4] {this.physics.initialPlaneVelocity, this.physics.initialVerticalVelocity, this.physics.planeAcceleration, this.physics.verticalAcceleration};
    }
}
