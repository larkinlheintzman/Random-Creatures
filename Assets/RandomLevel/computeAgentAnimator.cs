using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

[System.Serializable]
public class AgentColorSet
{

  public int count;
  public Color agentTrailColor;
  public Color agentSenseColor;
  public Vector2 startCenter;
  public float startRadius;

}


public class computeAgentAnimator : MonoBehaviour
{

  [SerializeField]
  public ComputeShader computeShader;
  [SerializeField]
  public int size;
  [SerializeField]
  public AgentColorSet[] agentColorSets;
  [SerializeField]
  public float diffuseRate = 0.1f;
  [SerializeField]
  public float decayRate = 0.1f;
  [SerializeField]
  public float trailWeight = 0.5f;
  [SerializeField]
  public float moveSpeed = 1.0f;
  [SerializeField]
  public float turnSpeed = 0.02f;
  [SerializeField]
  public float sensingAngleDeg = 20.0f;
  [SerializeField]
  public float sensingDist = 1.0f;
  [SerializeField, Range(-1.0f, 1.0f)]
  public float sensingWeight = 1.0f;
  [SerializeField]
  public float senseSize = 8.0f;
  [SerializeField]
  public float agentSize = 5.0f;
  [SerializeField]
  public float turnCooldown = 0.1f;
  [SerializeField, Range(0.0f, 1.0f)]
  public float headingAttractionSpeed = 0.1f;
  [SerializeField]
  public float turnRandomStrength = 0.1f;
  [SerializeField]
  public float turnRandomChance = 0.1f;
  [SerializeField]
  public float senseThreshold = 2f;
  [SerializeField,Range(0.00001f,3.141f)]
  public float headingIncrement = 3.14159f/4.0f;
  [SerializeField]
  public int frameIterations = 3;

  public AgentSettings[] agentSettings = new AgentSettings[1];

  [HideInInspector]
  public RenderTexture renderTexture;
  // public RenderTexture renderTextureRotation;
  public ComputeBuffer agentBuffer;
  public ComputeBuffer agentSettingsBuffer;

  [System.Serializable]
  public struct Agent
  {
    public float heading;
    public float2 position;
    public float bodySize;
    public float cooldown;
    public float cooldownCounter;
    public float sensingAngle;
    public float sensingWeight;
    public float sensingDist;
    public float senseSize;
    public float senseThreshold;
    public Color senseColor;
    public Color trailColor;
  };

  public struct AgentSettings
  {
    public float moveSpeed;
    public float turnSpeed;
    public float turnCooldown;
    public float turnRandomStrength;
    public float headingIncrement;
  };

  public Agent[] agents;

  public int GetAgentCount()
  {
    int numAgents = 0;
    for (int i = 0; i < agentColorSets.Length; i++) {
      numAgents += agentColorSets[i].count;
    }
    return numAgents;
  }

  public void SetUpAgents()
  {

    agentSettings[0].moveSpeed = moveSpeed;
    agentSettings[0].turnSpeed = turnSpeed;
    agentSettings[0].turnCooldown = turnCooldown;
    agentSettings[0].turnRandomStrength = turnRandomStrength;
    agentSettings[0].headingIncrement = headingIncrement;

    int numAgents = GetAgentCount();
    agents = new Agent[numAgents];

    int agentCounter = 0;
    for (int i = 0; i < agentColorSets.Length; i++)
    {
      for (int j = 0; j < agentColorSets[i].count; j++)
      {
        Vector2 vecStartPos;
        float vecStartHeading;
        float angle = 2*Mathf.PI*UnityEngine.Random.value;
        Vector2 circlePos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        vecStartPos = agentColorSets[i].startRadius*(circlePos) + agentColorSets[i].startCenter;
        vecStartHeading = angle;
        // Vector2 vecStartPos = new Vector2((renderTexture.width - buffer)*UnityEngine.Random.value + buffer, (renderTexture.width - buffer)*UnityEngine.Random.value + buffer);

        agents[agentCounter] = new Agent() {
          heading = vecStartHeading,
          position = vecStartPos,
          bodySize = agentSize,
          cooldown = 0.0f,
          cooldownCounter = 0.0f,
          sensingAngle = sensingAngleDeg*(Mathf.PI/180f),
          sensingDist = sensingDist,
          sensingWeight = sensingWeight,
          senseSize = senseSize,
          senseThreshold = senseThreshold,
          senseColor = agentColorSets[i].agentSenseColor,
          trailColor = agentColorSets[i].agentTrailColor,
        };
        agentCounter += 1;
      }
    }
    // for (int i = 0; i<agents.Length; i++) {
    //   // Vector2 vecStartPos = startRadius*(new Vector2(Mathf.Cos(theta*(180f/Mathf.PI)), Mathf.Sin(theta*(180f/Mathf.PI)))) + new Vector2(size/2f, size/2f);
    //   Vector2 vecStartPos;
    //   float vecStartHeading;
    //   vecStartPos = new Vector2((renderTexture.width - buffer)*UnityEngine.Random.value + buffer, (renderTexture.width - buffer)*UnityEngine.Random.value + buffer);
    //   vecStartHeading = UnityEngine.Random.value*Mathf.PI*2;
    //
    //   agents[i] = new Agent() {
    //     heading = vecStartHeading,
    //     position = vecStartPos,
    //     bodySize = agentSize,
    //     cooldown = 0.0f,
    //     cooldownCounter = 0.0f,
    //     sensingAngle = sensingAngleDeg*(Mathf.PI/180f),
    //     sensingDist = sensingDist,
    //     sensingWeight = sensingWeight,
    //     senseSize = senseSize,
    //     senseThreshold = senseThreshold,
    //     senseColor = senseColor,
    //     trailColor = trailColor,
    //   };
    //   theta += thetaStep;
    // }
    if (renderTexture != null)
    {
      renderTexture.Release();
    }
    renderTexture = new RenderTexture(size, size, 24);
    renderTexture.filterMode = FilterMode.Point;
    renderTexture.enableRandomWrite = true;
    renderTexture.Create();

    RawImage img = GetComponent<RawImage>();
    img.texture = renderTexture;

    int agentStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Agent));
    int agentSettingsStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(AgentSettings));

    agentBuffer = new ComputeBuffer(numAgents, agentStride);
    agentSettingsBuffer = new ComputeBuffer(numAgents, agentSettingsStride);

    // main kernel settings
    int mainKernel = computeShader.FindKernel("main");
    computeShader.SetBuffer(mainKernel, "Agents", agentBuffer);
    computeShader.SetBuffer(mainKernel, "agentSettings", agentSettingsBuffer);
    agentBuffer.SetData(agents);
    agentSettingsBuffer.SetData(agentSettings);

    computeShader.SetFloat("Resolution", size);
    computeShader.SetFloat("Time", Time.fixedTime);
    computeShader.SetFloat("DeltaTime", Time.fixedDeltaTime);
    computeShader.SetFloat("MoveSpeed", moveSpeed);
    computeShader.SetFloat("SenseThreshold", senseThreshold);
    computeShader.SetFloat("NumAgents", numAgents);
    computeShader.SetFloat("TrailWeight", trailWeight);
    computeShader.SetFloat("HeadingAttractionSpeed", headingAttractionSpeed);
    computeShader.SetTexture(mainKernel, "Output", renderTexture);
    computeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize); // not sure if necessary

    // diffuse kernel settings
    int diffuseKernel = computeShader.FindKernel("Diffuse");
    computeShader.SetTexture(diffuseKernel, "DiffusedOutput", renderTexture);
    computeShader.SetFloat("diffuseRate", diffuseRate);
    computeShader.SetFloat("decayRate", decayRate);
    // computeShader.SetFloat("Resolution", decayRate);

  }


  public void SetValues()
  {
    int mainKernel = computeShader.FindKernel("main");
    int diffuseKernel = computeShader.FindKernel("Diffuse");
    // int rotationKernel = computeShader.FindKernel("Rotation");

    computeShader.SetFloat("Resolution", size);
    computeShader.SetFloat("Time", Time.fixedTime);
    computeShader.SetFloat("DeltaTime", Time.fixedDeltaTime);
    computeShader.SetFloat("MoveSpeed", moveSpeed);
    computeShader.SetFloat("SenseThreshold", senseThreshold);
    // computeShader.SetFloat("NumAgents", GetAgentCount());
    computeShader.SetFloat("TrailWeight", trailWeight);
    computeShader.SetFloat("HeadingAttractionSpeed", headingAttractionSpeed);
    computeShader.SetTexture(mainKernel, "Output", renderTexture);
    computeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize); // not sure if necessary

    // diffuse kernel settings
    computeShader.SetTexture(diffuseKernel, "DiffusedOutput", renderTexture);
    computeShader.SetFloat("diffuseRate", diffuseRate);
    computeShader.SetFloat("decayRate", decayRate);

    agentSettings[0].moveSpeed = moveSpeed;
    agentSettings[0].turnSpeed = turnSpeed;
    agentSettings[0].turnCooldown = turnCooldown;
    agentSettings[0].turnRandomStrength = turnRandomStrength;
    agentSettings[0].headingIncrement = headingIncrement;

    // int agentSettingsStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(AgentSettings));
    // agentSettingsBuffer = new ComputeBuffer(numAgents, agentSettingsStride);

    agentSettingsBuffer.SetData(agentSettings);
    computeShader.SetBuffer(mainKernel, "agentSettings", agentSettingsBuffer);
  }

  public void Start()
  {
    SetUpAgents();
  }

  public void FixedUpdate()
  {
    SetValues();
    int mainKernel = computeShader.FindKernel("main");
    int diffuseKernel = computeShader.FindKernel("Diffuse");
    // int rotationKernel = computeShader.FindKernel("Rotation");
    computeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize); // not sure if necessary
    // ... assuming same kernel thread sizes

    for (int i = 0; i <= frameIterations; i++)
    {
      computeShader.Dispatch(mainKernel, renderTexture.width/(int)xGroupSize, renderTexture.height/(int)yGroupSize, 1);
    }
    computeShader.Dispatch(diffuseKernel, renderTexture.width/(int)xGroupSize, renderTexture.height/(int)yGroupSize, 1);

    // only need to get rotation once
    // computeShader.SetTexture(rotationKernel, "Input", renderTexture); // may need to get this after iterations are done
    // computeShader.Dispatch(rotationKernel, renderTexture.width/(int)xGroupSize, renderTexture.height/(int)yGroupSize, 1);

  }

  void OnDestroy()
  {
    agentBuffer.Release();
    agentSettingsBuffer.Release();
  }
}
