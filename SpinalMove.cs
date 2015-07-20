using UnityEngine;
using System.Collections;

public class SpinalMove : MonoBehaviour {

	[Range(3,20)]
	public int numberOfBones = 7;
	
	[Range(1f,100f)]
	public float length = 7f;
	
	public float anchorOffset = 1f;
	
	[Range(30f,170f)]
	public float minAngle;
	
	[Range(0.1f,0.9f)]
	public float stiffness = 0.3f;
	
	Vector2[] bones;
	
	float boneLength;
	float boneLengthSqr;
	
	float stepLength;
	public int maxStep = 50;
	
	//Steer locomotion
	//public float maxSteerAngle;

	void Start () {
	
		boneLength = length / numberOfBones;
		boneLengthSqr = boneLength * boneLength;
		stepLength = boneLength / maxStep;
		Debug.Log(stepLength);
	
		if (bones==null) {
			bones = new Vector2[numberOfBones];
			Vector2 pos = this.transform.position;
			for (int i=0;i<numberOfBones;i++) {
				bones[i] = pos + (Vector2)this.transform.up * boneLength * i;
			}
		} 
	}
	
	void Update(){

		if (bones == null) return;
		Vector2 prePos = bones[0];
		bones[0] = this.transform.position + this.transform.up * anchorOffset;
		
		//position constraint
		for (int i=1;i<numberOfBones;i++) {
			Vector2 newPos = Move (bones[i],prePos, bones[i-1]);
			prePos = bones[i];
			bones[i] = newPos;
		}
		
		//minAngle constraint
		for (int i=2;i<numberOfBones;i++) {
			bones[i] = Rotate(bones[i],bones[i-1],bones[i-2]);
		}
	}
	
	Vector2 Move(Vector2 curPos, Vector2 targetPrePos, Vector2 targetCurPos) {
		
		Vector2 dir = targetPrePos - targetCurPos;
		float distSqr = dir.SqrMagnitude();
		if (distSqr>boneLengthSqr) {
			return dir.normalized * boneLength + targetCurPos;
		} else {
			Vector2 dir2 = targetCurPos - curPos;
			float dist2 = dir2.SqrMagnitude();
			Vector2 step;
			if (dist2 == boneLengthSqr) {
				return curPos;
			} else if (dist2 > boneLengthSqr) {
				step = (targetPrePos - curPos).normalized;
				for (int j=0;j<maxStep;j++) {
					curPos += (step*stepLength);
					if ((curPos-targetCurPos).SqrMagnitude() <= boneLengthSqr) return curPos;
				}
				return curPos;
			} else {
				step = (curPos - targetPrePos).normalized;
				for (int j=0;j<maxStep;j++) {
					curPos += (step*stepLength);
					if ((curPos-targetCurPos).SqrMagnitude() >= boneLengthSqr) return curPos;
				}
				return curPos;
			}
		}
	}
	
	Vector2 Rotate ( Vector2 cur, Vector2 pivot , Vector2 target){
		float a = GetRelativeAngle(cur,pivot,target);
		if (Mathf.Abs(a) > 180f - minAngle) {
			float deltaA;
			if (a>0) deltaA = a - 180f + minAngle;
			else deltaA = a - minAngle + 180;
			//deltaA = Mathf.Clamp(deltaA,-20,20f);
			cur = RotatePoint2D ( cur,pivot,-deltaA * stiffness );
			return cur;
		} else return cur;
	}
	
	float GetRelativeAngle(Vector2 curPos,Vector2 targetPre, Vector2 targetCur){
		Vector2 tp2tc = targetCur - targetPre;
		Vector2 cur2Tpre = targetPre - curPos;
		Vector3 t = Vector3.Cross(tp2tc,cur2Tpre);
		float a = Vector2.Angle(tp2tc,cur2Tpre);
		if (t.z<0f) a=-a;
		return a;
	}
	
	void OnDrawGizmos() {
		if (bones!=null) {
			Gizmos.color = Color.yellow;
			for (int i=0;i<bones.Length;i++) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(bones[i],0.1f);
				Gizmos.color = Color.green;
				if (i<=bones.Length -2) 
					Gizmos.DrawLine(bones[i],bones[i+1]);
			}
		}
		
		if (Application.isPlaying==false) {
			float boneL = length / numberOfBones;
			Vector2 posP,posN;
			posP = (Vector2)(this.transform.position + this.transform.up * anchorOffset);
			Gizmos.DrawWireSphere(posP,0.1f);
			for (int i=0;i<numberOfBones;i++) {
				posN = posP - (Vector2)this.transform.up * boneL;
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(posN,0.1f);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(posP,posN);
				posP = posN;
			}
		}
	}
	
	public static Vector2 RotatePoint2D(Vector2 point,Vector2 axis,float angle) {
		angle *= Mathf.Deg2Rad; 
		float angleCos = Mathf.Cos(angle);
		float angleSin = Mathf.Sin(angle);
		float px = angleCos * (point.x - axis.x) - angleSin * (point.y - axis.y) + axis.x;
		float py = angleSin * (point.x - axis.x) + angleCos * (point.y - axis.y) + axis.y;
		return new Vector2 (px,py);
	}
	
}
