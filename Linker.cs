using UnityEngine;
using System.Collections.Generic;

public class Linker : MonoBehaviour {
  
  [SerializeField]
  private GameObject debugPoint;
  
  private Collider2D trigonCol;
  private Animator trigonAnimator;
  
  private int index;
  private Vector2[] triangle;
  
  [Header("Debug")]
  public float legA;
  public float legB;
  public Vector2 dir = Vector2.zero;
  public float ang;
  
  public void Init(int index, Vector2[] triangle) {
    this.index = index;
    this.triangle = triangle;
  }
  
  public void UpdateLegs() {
    Vector2 pos = triangle[index];
    legA = -Vector2.SignedAngle(Vector2.up, triangle[(index + 1) % triangle.Length] - triangle[index]);
    legB = -Vector2.SignedAngle(Vector2.up, triangle[(index + 2) % triangle.Length] - triangle[index]);
  }
  
  private bool CheckAngle(float ang) {
    // transform.parent.RotateAround(transform.position, Vector3.forward, ang);
    this.ang = ang;
    return true;
  }
  
  private Vector2[] ghost = new Vector2[3];
  private void Validate(float ang, Vector2 dir) {
    System.Array.Copy(triangle, ghost, 3);
    
    for (int i = 0; i < ghost.Length; i++) {
      ghost[i] = Quaternion.AngleAxis(transform.rotation.eulerAngles.z + ang, Vector3.forward) * ghost[i];
      ghost[i] += (Vector2)transform.position;
      ghost[i] += dir;
      
      Instantiate(debugPoint, ghost[i] + (Vector2)transform.position, Quaternion.identity);
    }
    bool valid = true;
  }
  
  private void Awake() {
    trigonCol = transform.parent.GetComponent<Collider2D>();
    trigonAnimator = transform.parent.GetComponent<Animator>();
  }
  
  private void OnTriggerStay2D(Collider2D col) {
    if (!trigonAnimator.GetBool("Selected")) return;
    Linker other = col.gameObject.GetComponent<Linker>();
    float rot = transform.rotation.eulerAngles.z;
    float otherRot = other.transform.rotation.eulerAngles.z;
    
    float ab = Mathf.DeltaAngle(other.legB - otherRot, legA - rot);
    float ba = Mathf.DeltaAngle(other.legA - otherRot, legB - rot);
    Vector2 dir = col.transform.position - transform.position;
    // ang = (Mathf.Abs(ab) < Mathf.Abs(ba)) ? ab : ba;
    
    Transform parent = transform.parent;
    parent.position += (Vector3)dir;
    // transform.parent.RotateAround(transform.position, Vector3.forward, 90f);
    if (Mathf.Abs(ab) < Mathf.Abs(ba)) {
      if (CheckAngle(ab) || CheckAngle(ba)) {
        this.dir = dir;
        Validate(ang, dir);
      } else {
        this.dir = Vector2.negativeInfinity;
        ang = Mathf.NegativeInfinity;
      }
    } else {
      if (CheckAngle(ba) || CheckAngle(ab)) {
        this.dir = dir;
        Validate(ang, dir);
      } else {
        this.dir = Vector2.negativeInfinity;
        ang = Mathf.NegativeInfinity;
      }
    }
    parent.position -= (Vector3)dir;
    // transform.parent.RotateAround(transform.position, Vector3.forward, -90f);
  }
  
  private void OnTriggerExit2D(Collider2D col) {
    dir = Vector2.negativeInfinity;
    ang = Mathf.NegativeInfinity;
  }
}