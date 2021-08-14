using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class cityGridRunner : MonoBehaviour
{

  [SerializeField]
  public ComputeShader computeShader;
  [SerializeField]
  public int size;
  [SerializeField]
  public Color wallColor = Color.black;
  [SerializeField]
  public Color spaceColor = Color.white;
  [SerializeField]
  public int numAgents = 10;
  [SerializeField]
  public float diffuseRate = 0.1f;
  [SerializeField]
  public float decayRate = 0.1f;
  [SerializeField]
  public float trailWeight = 0.5f;
  [SerializeField]
  public float startRadius = 0.1f;
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
  public float sensingSize = 8.0f;
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
  public int iterationsPerFrame = 2;
  [SerializeField]
  public int totalIterations = 200;


  public AgentSettings[] agentSettings = new AgentSettings[1];

  public RenderTexture renderTexture;
  public RenderTexture renderTextureRotation;
  public ComputeBuffer agentBuffer;
  public ComputeBuffer agentSettingsBuffer;

  public struct Agent
  {
    public float heading;
    public float2 position;
    public float cooldown;
    public float cooldownCounter;
  };

  public struct AgentSettings
  {
    public float moveSpeed;
    public float turnSpeed;
    public float sensingAngle;
    public float sensingDist;
    public float sensingWeight;
    public float sensingSize;
    public float turnCooldown;
    public float turnRandomStrength;
    public float turnRandomChance;
    public float headingIncrement;
  };
  private Agent[] agents;
  public bool gridReady = false;
  public float waitTime = 0.1f;
  public float waitTimeCounter = 0.0f;
  public int iterationCounter = 0;

  // IEnumerator waiter()
  // {
  //   yield return new WaitForSeconds(3f);
  //
  // }

  public void RunCityGrid()
  {

    gridReady = false;

    agentSettings[0].moveSpeed = moveSpeed;
    agentSettings[0].turnSpeed = turnSpeed;

    agentSettings[0].sensingAngle = sensingAngleDeg*(Mathf.PI/180.0f);
    agentSettings[0].sensingDist = sensingDist;
    agentSettings[0].sensingWeight = sensingWeight;
    agentSettings[0].sensingSize = sensingSize;
    agentSettings[0].turnCooldown = turnCooldown;
    agentSettings[0].turnRandomStrength = turnRandomStrength;
    agentSettings[0].turnRandomChance = turnRandomChance;
    agentSettings[0].headingIncrement = headingIncrement;

    agents = new Agent[numAgents];
    // agentSettings = new AgentSettings[numAgents];

    float theta = 0.0f;
    float buffer = 10.0f;
    float thetaStep = (2*Mathf.PI)/numAgents;

    for (int i = 0; i<agents.Length; i++) {
      // Vector2 vecStartPos = startRadius*(new Vector2(Mathf.Cos(theta*(180f/Mathf.PI)), Mathf.Sin(theta*(180f/Mathf.PI)))) + new Vector2(size/2f, size/2f);
      Vector2 vecStartPos;
      float vecStartHeading;
      vecStartPos = new Vector2((size - buffer)*UnityEngine.Random.value + buffer, (size - buffer)*UnityEngine.Random.value + buffer);
      vecStartHeading = UnityEngine.Random.value*Mathf.PI*2;
      // if (UnityEngine.Random.value >= 0.5f)
      // {
      //   vecStartPos = new Vector2((size - buffer)*UnityEngine.Random.value + buffer, size - buffer);
      //   vecStartHeading = -Mathf.PI/2.0f + startRadius*(UnityEngine.Random.value - 0.5f);
      // }
      // else
      // {
      //   vecStartPos = new Vector2((size - buffer)*UnityEngine.Random.value + buffer, buffer);
      //   vecStartHeading = Mathf.PI/2.0f + startRadius*(UnityEngine.Random.value - 0.5f);
      // }
      // Debug.Log("making agent at: ");
      // Debug.Log(vecStartPos);

      agents[i] = new Agent() {
        heading = vecStartHeading,
        position = vecStartPos,
        cooldown = 0.0f,
        cooldownCounter = 0.0f
      };
      theta += thetaStep;
      // agents[i].heading = 0;
      // agents[i].position = new Vector2(size/2f, size/2f);
    }

    if (renderTexture != null)
    {
      renderTexture.Release();
    }
    renderTexture = new RenderTexture(size, size, 24);
    renderTexture.filterMode = FilterMode.Point;
    renderTexture.enableRandomWrite = true;
    renderTexture.Create();

    if (renderTextureRotation != null)
    {
      renderTextureRotation.Release();
    }
    renderTextureRotation = new RenderTexture(size, size, 24);
    renderTextureRotation.filterMode = FilterMode.Point;
    renderTextureRotation.enableRandomWrite = true;
    renderTextureRotation.Create();

    int agentStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Agent));
    int agentSettingsStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(AgentSettings));

    agentBuffer = new ComputeBuffer(numAgents, agentStride);
    agentSettingsBuffer = new ComputeBuffer(numAgents, agentSettingsStride);

    // main kernel settings
    int mainKernel = computeShader.FindKernel("main");
    computeShader.SetBuffer(mainKernel, "Agents", agentBuffer);
    computeShader.SetBuffer(mainKernel, "agentSettings", agentSettingsBuffer);
    agentBuffer.SetData(agents);
    computeShader.SetFloat("Resolution", renderTexture.width);
    computeShader.SetFloat("Time", Time.fixedTime);
    computeShader.SetFloat("DeltaTime", Time.fixedDeltaTime);
    computeShader.SetFloat("MoveSpeed", moveSpeed);
    computeShader.SetFloat("SenseThreshold", senseThreshold);
    computeShader.SetFloat("NumAgents", numAgents);
    computeShader.SetFloat("TrailWeight", trailWeight);
    computeShader.SetFloat("HeadingAttractionSpeed", headingAttractionSpeed);
    computeShader.SetVector("WallColor", wallColor);
    computeShader.SetVector("SpaceColor", spaceColor);
    agentSettingsBuffer.SetData(agentSettings);
    computeShader.SetTexture(mainKernel, "Output", renderTexture);
    computeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize); // not sure if necessary

    // diffuse kernel settings
    int diffuseKernel = computeShader.FindKernel("Diffuse");
    computeShader.SetTexture(diffuseKernel, "DiffusedOutput", renderTexture);
    computeShader.SetFloat("diffuseRate", diffuseRate);
    computeShader.SetFloat("decayRate", decayRate);

    // rotation kernel settings
    int rotationKernel = computeShader.FindKernel("Rotation");
    computeShader.SetTexture(rotationKernel, "RotatedOutput", renderTextureRotation);
    // computeShader.SetInt("sampleIncrements", rotationSampleIncrements);
    // computeShader.SetFloat("sampleDistance", rotationSampleDistance);


    for (int i = 0; i <= totalIterations; i++)
    {
      // ... assuming same kernel thread sizes
      computeShader.Dispatch(mainKernel, renderTexture.width/(int)xGroupSize, renderTexture.height/(int)yGroupSize, 1);
      computeShader.Dispatch(diffuseKernel, renderTexture.width/(int)xGroupSize, renderTexture.height/(int)yGroupSize, 1);
    }

    // only need to get rotation once
    computeShader.SetTexture(rotationKernel, "Input", renderTexture); // may need to get this after iterations are done
    computeShader.Dispatch(rotationKernel, renderTexture.width/(int)xGroupSize, renderTexture.height/(int)yGroupSize, 1);

    gridReady = true;

  }

  // void LateUpdate()
  // {
  //
  //
  // }

  void OnDestroy()
  {
    agentBuffer.Release();
    agentSettingsBuffer.Release();
  }

  //
  // // Update is called once per frame
  // void Update()
  // {
  //
  // }
}
