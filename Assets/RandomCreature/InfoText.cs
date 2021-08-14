using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoText : MonoBehaviour
{
  // ui things
  public Vector3 offset;
  public Text text;
  public RectTransform textTransform;
  public float distanceFromTarget = 0.4f;
  public Transform target; // thing that controls text position
  public int id;

  public Vector3 referenceVelocity = Vector3.zero;
  public float smoothTime = 0.02f;

  private bool initalized;
  private Canvas myCanvas;
  private GameObject textContainer;

  private LineRenderer lineRenderer;
  private string[] Alphabet = new string[26] {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"};

  private Material lineMaterial;
  private CreatureGenerator generator;

  public void Initialize(Canvas canvas, Transform target, CreatureGenerator gen, int id)
  {
    this.id = id;
    this.generator = gen;
    this.myCanvas = canvas;
    this.target = target;
    this.offset = target.position - generator.transform.position;
    this.offset.y = 0.0f;
    // this.offset = new Vector3(0f,-0.27f,-1.5f);


    this.text = gameObject.AddComponent<Text>();
    this.text.font = (Font)Resources.Load("techFont");
    this.text.text = "<Error>_,";
    this.text.color = Color.black;
    this.text.fontSize = 46;
    this.text.horizontalOverflow = HorizontalWrapMode.Overflow;
    this.text.verticalOverflow = VerticalWrapMode.Overflow;

    this.textTransform = GetComponent<RectTransform>();
    this.textTransform.localScale = Vector3.one;
    this.textTransform.SetParent(canvas.transform);

    this.lineRenderer = gameObject.AddComponent<LineRenderer>();
    // lineMaterial = (Material)Resources.Load("Spark", typeof(Material));
    // Debug.Log(lineMaterial);
    // this.lineRenderer.materials[0] = lineMaterial;
    this.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    this.lineRenderer.materials[0] = new Material(Shader.Find("Sprites/Default"));
    // this.lineRenderer.material = new Material(Shader.Find("Spark"));
    this.lineRenderer.widthMultiplier = 0.1f;
    this.lineRenderer.positionCount = 2;
    this.lineRenderer.numCapVertices = 50;
    this.lineRenderer.numCornerVertices = 50;

    // A simple 2 color gradient with a fixed alpha of 1.0f.
    float alpha = 1.0f;




    Gradient gradient = new Gradient();
    // gradient = UnityEditor.GradientWrapperJSON:{"gradient":{"serializedVersion":"2","key0":{"r":0.0,"g":0.7557437419891357,"b":1.0,"a":1.0},"key1":{"r":0.0,"g":0.7437029480934143,"b":1.0,"a":1.0},"key2":{"r":0.0,"g":0.14755606651306153,"b":1.0,"a":0.0},"key3":{"r":1.0,"g":0.8276978731155396,"b":0.0,"a":0.0},"key4":{"r":0.0,"g":0.0,"b":0.0,"a":0.0},"key5":{"r":0.0,"g":0.0,"b":0.0,"a":0.0},"key6":{"r":0.0,"g":0.0,"b":0.0,"a":0.0},"key7":{"r":0.0,"g":0.0,"b":0.0,"a":0.0},"ctime0":0,"ctime1":25443,"ctime2":65535,"ctime3":64186,"ctime4":0,"ctime5":0,"ctime6":0,"ctime7":0,"atime0":0,"atime1":65535,"atime2":0,"atime3":0,"atime4":0,"atime5":0,"atime6":0,"atime7":0,"m_Mode":0,"m_NumColorKeys":3,"m_NumAlphaKeys":2}};

    gradient.SetKeys(
        // new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.red, 1.0f) },
        new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.black, 1.0f) },
        new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
    );
    this.lineRenderer.colorGradient = gradient;
    // this.lineRenderer.colorGradient = gradient;

    this.initalized = true;
  }

  public void TextEnable(bool on)
  {
    this.text.enabled = on; // orn or oarf
    this.lineRenderer.enabled = on;
    // if (on)
    // {
    //   lineRenderer.material = lineMaterial;
    // }
  }

  // Update is called once per frame
  public Vector3 playerBoundingBox = new Vector3(0.2f, 0.2f, 0.2f);
  void FixedUpdate()
  {
    if (initalized)
    {
      // todo map local offset to direction facing?
      offset = target.position - generator.transform.position;
      offset.y = 0.0f;
      // Vector3 mappedOffset = generator.cameraTransform.TransformVector(offset);
      Vector3 mappedOffset = offset;
      mappedOffset.Normalize();
      mappedOffset = mappedOffset*distanceFromTarget;

      Vector3 textPosition = target.position + mappedOffset;
      // textPosition.x =
      // textPosition.x = Mathf.Clamp(textPosition.x, target.transform.position.x + playerBoundingBox.x, Mathf.Infinity);
      // // textPosition.y = Mathf.Clamp(textPosition.y, target.transform.position.y + playerBoundingBox.y, Mathf.Infinity);
      // textPosition.z = Mathf.Clamp(textPosition.z, target.transform.position.z + playerBoundingBox.z, Mathf.Infinity);
      transform.position = Vector3.SmoothDamp(transform.position, textPosition, ref referenceVelocity, smoothTime);
      // transform.position = target.position + localOffset;

      var pts = new Vector3[2];
      if (text.enabled)
      {
        string newText = id.ToString() + ": ";
        for (int j = 0; j < 5; j++) {
          newText = string.Concat(newText, Alphabet[Random.Range(0, Alphabet.Length)]);
        }
        text.text = newText;

        pts[0] = target.position;
        pts[1] = transform.position;
        lineRenderer.SetPositions(pts);

        transform.localEulerAngles = Vector3.zero;

      }
    }

  }
}
