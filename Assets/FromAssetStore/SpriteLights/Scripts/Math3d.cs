
using UnityEngine;
using System.Collections;
using System;

/*
This script contains a collection of 3d geometric functions.

Note: 
-All vector inputs which indicate a direction must be normalized first.
*/
public class Math3d : MonoBehaviour {

	private static Transform tempChild = null;
	private static Transform tempParent = null;

	//Call this first if TransformWithParent is used
	private static void Init(){

		tempChild = (new GameObject("Math3d_TempChild")).transform;
		tempParent = (new GameObject("Math3d_TempParent")).transform;

		tempChild.gameObject.hideFlags = HideFlags.HideAndDontSave;
		DontDestroyOnLoad(tempChild.gameObject);

		tempParent.gameObject.hideFlags = HideFlags.HideAndDontSave;
		DontDestroyOnLoad(tempParent.gameObject);

		//set the parent
		tempChild.parent = tempParent;
	}
	
	//Get y from a linear function, with x as an input. The linear function goes through points
	//Pxy on the left ,and Qxy on the right.
	public static float LinearFunction(float x, float Px, float Py, float Qx, float Qy){

		float y = 0f;

		float A = Qy -Py;
		float B = Qx - Px;
		float C = A / B;

		y = Py + (C * (x - Px));

		return y;
	}

	//Convert the input to a range of 0 and 1.
	//Use this function if the lowest possible value of the input is not 0.
	public static float NormalizeComplex(float input, float lowest, float highest){

		return (input - lowest) / (highest - lowest);
	}

	//Convert the input to a range of 0 and 1.
	//Use this function if the lowest possible value of the input is 0.
	public static float NormalizeSimple(float input, float highest){

		return input / highest;
	}

	//This gets a value with a range from 0 to 1 and changes it to a range of 1 to 0
	public static float NormalizeInvert(float input){

		return -input + 1f;
	}

	//increase or decrease the length of vector by size
	public static Vector3 AddVectorLength(Vector3 vector, float size){
		
		//get the vector length
		float magnitude = Vector3.Magnitude(vector);

		//change the length
		magnitude += size;

		//normalize the vector
		Vector3 vectorNormalized = Vector3.Normalize(vector);

		//scale the vector
		return Vector3.Scale(vectorNormalized, new Vector3(magnitude, magnitude, magnitude));		
	}

	//create a vector of direction "vector" with length "size"
	public static Vector3 SetVectorLength(Vector3 vector, float size){
		
		//normalize the vector
		Vector3 vectorNormalized = Vector3.Normalize(vector);

		//scale the vector
		return vectorNormalized *= size;
	}


	//calculate the rotational difference from A to B
	public static Quaternion SubtractRotation(Quaternion B, Quaternion A){

		Quaternion C = Quaternion.Inverse(A) * B;		
		return C;
	}

	//Add rotation B to rotation A.
	public static Quaternion AddRotation(Quaternion A, Quaternion B){

		Quaternion C = A * B;		
		return C;
	}

	//Same as the build in TransformDirection(), but using a rotation instead of a transform
	public static Vector3 TransformDirectionMath(Quaternion rotation, Vector3 vector){

		Vector3 output = rotation * vector;
		return output;
	}

	//Same as the build in InverseTransformDirection(), but using a rotation instead of a transform
	public static Vector3 InverseTransformDirectionMath(Quaternion rotation, Vector3 vector){

		Vector3 output = Quaternion.Inverse(rotation) * vector; 
		return output;
	}

	//Rotate a vector as if it is attached to an object with rotation "from", which is then rotated to rotation "to".
	//Similar to TransformWithParent(), but rotating a vector instead of a transform.
	public static Vector3 RotateVectorFromTo(Quaternion from, Quaternion to, Vector3 vector){
																//Note: comments are in case all inputs are in World Space.
		Quaternion Q = SubtractRotation(to, from);				//Output is in object space.
		Vector3 A = InverseTransformDirectionMath(from, vector);//Output is in object space.
		Vector3 B = Q * A;										//Output is in local space.
		Vector3 C = TransformDirectionMath(from, B);			//Output is in world space.
		return C;
	}

	//Find the line of intersection between two planes.	The planes are defined by a normal and a point on that plane.
	//The outputs are a point on the line and a vector which indicates it's direction. If the planes are not parallel, 
	//the function outputs true, otherwise false.
	public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal, Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position){
		
		linePoint = Vector3.zero;
		lineVec = Vector3.zero;

		//We can get the direction of the line of intersection of the two planes by calculating the 
		//cross product of the normals of the two planes. Note that this is just a direction and the line
		//is not fixed in space yet. We need a point for that to go with the line vector.
		lineVec = Vector3.Cross(plane1Normal, plane2Normal);

		//Next is to calculate a point on the line to fix it's position in space. This is done by finding a vector from
		//the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
		//errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
		//the cross product of the normal of plane2 and the lineDirection.		
		Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);		

		float denominator = Vector3.Dot(plane1Normal, ldir);

		//Prevent divide by zero and rounding errors by requiring about 5 degrees angle between the planes.
		if(Mathf.Abs(denominator) > 0.006f){
			
			Vector3 plane1ToPlane2 = plane1Position - plane2Position;
			float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / denominator;
			linePoint = plane2Position + t * ldir;
			
			return true;
		}

		//output not valid
		else{
			return false;
		}
	}	

	//Get the intersection between a line and a plane. 
	//If the line and plane are not parallel, the function outputs true, otherwise false.
	public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint){
		
		float length;
		float dotNumerator;
		float dotDenominator;
		Vector3 vector;
		intersection = Vector3.zero;

		//calculate the distance between the linePoint and the line-plane intersection point
		dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
		dotDenominator = Vector3.Dot(lineVec, planeNormal);

		//line and plane are not parallel
		if(dotDenominator != 0.0f){
			length =  dotNumerator / dotDenominator;
			
			//create a vector from the linePoint to the intersection point
			vector = SetVectorLength(lineVec, length);
			
			//get the coordinates of the line-plane intersection point
			intersection = linePoint + vector;	
			
			return true;	
		}

		//output not valid
		else{
			return false;
		}
	}

	//Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
	//Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
	//same plane, use ClosestPointsOnTwoLines() instead.
	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		intersection = Vector3.zero;

		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

		//Lines are not coplanar. Take into account rounding errors.
		if((planarFactor >= 0.00001f) || (planarFactor <= -0.00001f)){

			return false;
		}

		//Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
		float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

		if((s >= 0.0f) && (s <= 1.0f)){

			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}

		else{
			return false;	   
		}
	}



	//Two non-parallel lines which may or may not touch each other have a point on each line which are closest
	//to each other. This function finds those two points. If the lines are not parallel, the function 
	//outputs true, otherwise false.
	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;

		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);

		float d = a*e - b*b;

		//lines are not parallel
		if(d != 0.0f){
			
			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);
			
			float s = (b*f - c*e) / d;
			float t = (a*f - c*b) / d;
			
			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;
			
			return true;
		}

		else{
			return false;
		}
	}	

	//This function returns a point which is a projection from a point to a line.
	//The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point){		
		
		//get vector from point on line to point in space
		Vector3 linePointToPoint = point - linePoint;

		float t = Vector3.Dot(linePointToPoint, lineVec);

		return linePoint + lineVec * t;
	}	

	//This function returns a point which is a projection from a point to a plane.
	public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point){
		
		float distance;
		Vector3 translationVector;

		//First calculate the distance from the point to the plane:
		distance = SignedDistancePlanePoint(planeNormal, planePoint, point);

		//Reverse the sign of the distance
		distance *= -1;

		//Get a translation vector
		translationVector = SetVectorLength(planeNormal, distance);

		//Translate the point to form a projection
		return point + translationVector;
	}	

	//Projects a vector onto a plane. The output is not normalized.
	public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector){
		
		return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
	}

	//Get the shortest distance between a point and a plane. The output is signed so it holds information
	//as to which side of the plane normal the point is.
	public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point){
		
		return Vector3.Dot(planeNormal, (point - planePoint));
	}	

	//This function calculates a signed (+ or - sign instead of being ambiguous) dot product. It is basically used
	//to figure out whether a vector is positioned to the left or right of another vector. The way this is done is
	//by calculating a vector perpendicular to one of the vectors and using that as a reference. This is because
	//the result of a dot product only has signed information when an angle is transitioning between more or less
	//then 90 degrees.
	public static float SignedDotProduct(Vector3 vectorA, Vector3 vectorB, Vector3 normal){

		Vector3 perpVector;
		float dot;

		//Use the geometry object normal and one of the input vectors to calculate the perpendicular vector
		perpVector = Vector3.Cross(normal, vectorA);

		//Now calculate the dot product between the perpendicular vector (perpVector) and the other input vector
		dot = Vector3.Dot(perpVector, vectorB);

		return dot;
	}

	//Calculate the angle between a vector and a plane. The plane is made by a normal vector.
	//Output is in radians.
	public static float AngleVectorPlane(Vector3 vector, Vector3 normal){
		
		float dot;
		float angle;

		//calculate the the dot product between the two input vectors. This gives the cosine between the two vectors
		dot = Vector3.Dot(vector, normal);

		//this is in radians
		angle = (float)Math.Acos(dot);

		return 1.570796326794897f - angle; //90 degrees - angle
	}

	//Calculate the dot product as an angle
	public static float DotProductAngle(Vector3 vec1, Vector3 vec2){
		
		double dot;
		double angle;

		//get the dot product
		dot = Vector3.Dot(vec1, vec2);

		//Clamp to prevent NaN error. Shouldn't need this in the first place, but there could be a rounding error issue.
		if(dot < -1.0f){
			dot = -1.0f;
		}							
		if(dot > 1.0f){
			dot =1.0f;
		}

		//Calculate the angle. The output is in radians
		//This step can be skipped for optimization...
		angle = Math.Acos(dot);

		return (float)angle;
	}

	//Convert a plane defined by 3 points to a plane defined by a vector and a point. 
	//The plane point is the middle of the triangle defined by the 3 points.
	public static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA, Vector3 pointB, Vector3 pointC){

		planeNormal = Vector3.zero;
		planePoint = Vector3.zero;

		//Make two vectors from the 3 input points, originating from point A
		Vector3 AB = pointB - pointA;
		Vector3 AC = pointC - pointA;

		//Calculate the normal
		planeNormal = Vector3.Normalize(Vector3.Cross(AB, AC));

		//Get the points in the middle AB and AC
		Vector3 middleAB = pointA + (AB / 2.0f);
		Vector3 middleAC = pointA + (AC / 2.0f);

		//Get vectors from the middle of AB and AC to the point which is not on that line.
		Vector3 middleABtoC = pointC - middleAB;
		Vector3 middleACtoB = pointB - middleAC;

		//Calculate the intersection between the two lines. This will be the center 
		//of the triangle defined by the 3 points.
		//We could use LineLineIntersection instead of ClosestPointsOnTwoLines but due to rounding errors 
		//this sometimes doesn't work.
		Vector3 temp;
		ClosestPointsOnTwoLines(out planePoint, out temp, middleAB, middleABtoC, middleAC, middleACtoB);
	}

	//Returns the forward vector of a quaternion
	public static Vector3 GetForwardVector(Quaternion q){

		return q * Vector3.forward;
	}

	//Returns the up vector of a quaternion
	public static Vector3 GetUpVector(Quaternion q){
	
		return q * Vector3.up;
	}

	//Returns the right vector of a quaternion
	public static Vector3 GetRightVector(Quaternion q){

		return q * Vector3.right;
	}

	//Gets a quaternion from a matrix
	public static Quaternion QuaternionFromMatrix(Matrix4x4 m){ 

		return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); 
	}

	//Gets a position from a matrix
	public static Vector3 PositionFromMatrix(Matrix4x4 m){

		Vector4 vector4Position = m.GetColumn(3);
		return new Vector3(vector4Position.x, vector4Position.y, vector4Position.z);
	}

	//This is an alternative for Quaternion.LookRotation. Instead of aligning the forward and up vector of the game 
	//object with the input vectors, a custom direction can be used instead of the fixed forward and up vectors.
	//alignWithVector and alignWithNormal are in world space.
	//customForward and customUp are in object space.
	//Usage: use alignWithVector and alignWithNormal as if you are using the default LookRotation function.
	//Set customForward and customUp to the vectors you wish to use instead of the default forward and up vectors.
	public static void LookRotationExtended(ref GameObject gameObjectInOut, Vector3 alignWithVector, Vector3 alignWithNormal, Vector3 customForward, Vector3 customUp){
		
		//Set the rotation of the destination
		Quaternion rotationA = Quaternion.LookRotation(alignWithVector, alignWithNormal);		

		//Set the rotation of the custom normal and up vectors. 
		//When using the default LookRotation function, this would be hard coded to the forward and up vector.
		Quaternion rotationB = Quaternion.LookRotation(customForward, customUp);

		//Calculate the rotation
		gameObjectInOut.transform.rotation =  rotationA * Quaternion.Inverse(rotationB);
	}

	//This function transforms one object as if it was parented to the other.
	//Note: before using this function, the Init() function must be called
	//Input: parentRotation and parentPosition: the current (new) parent transform.
	//Input: startParentRotation and startParentPosition: the transform of the parent object at the time the objects are parented.
	//Input: startChildRotation and startChildPosition: the transform of the child object at the time the objects are parented.
	//Output: childRotation and childPosition.
	//All transforms are in world space.
	public static void TransformWithParent(out Quaternion childRotation, out Vector3 childPosition, Quaternion parentRotation, Vector3 parentPosition, Quaternion startParentRotation, Vector3 startParentPosition, Quaternion startChildRotation, Vector3 startChildPosition){
		
		childRotation = Quaternion.identity; 
		childPosition = Vector3.zero;

		if(tempParent == null){

			Init();
		}

		//set the parent start transform
		tempParent.transform.rotation = startParentRotation;
		tempParent.transform.position = startParentPosition;
		tempParent.transform.localScale = Vector3.one; //to prevent scale wandering

		//set the child start transform
		tempChild.transform.rotation = startChildRotation;
		tempChild.transform.position = startChildPosition;
		tempChild.transform.localScale = Vector3.one; //to prevent scale wandering

		//translate and rotate the child by moving the parent
		tempParent.transform.rotation = parentRotation;
		tempParent.transform.position = parentPosition;

		//get the child transform
		childRotation = tempChild.transform.rotation;
		childPosition = tempChild.transform.position;
	
	}

	//With this function you can align a triangle of an object with any transform.
	//Usage: gameObjectInOut is the game object you want to transform.
	//alignWithVector, alignWithNormal, and alignWithPosition is the transform with which the triangle of the object should be aligned with.
	//triangleDirection, triangleNormal, and trianglePosition is the transform of the triangle from the object.
	//alignWithVector, alignWithNormal, and alignWithPosition are in world space.
	//triangleDirection, triangleNormal, and trianglePosition are in object space.
	//trianglePosition is the mesh position of the triangle. The effect of the scale of the object is handled automatically.
	//trianglePosition can be set at any position, it does not have to be at a vertex or in the middle of the triangle.
	public static void PreciseAlign(ref GameObject gameObjectInOut, Vector3 alignWithVector, Vector3 alignWithNormal, Vector3 alignWithPosition, Vector3 triangleDirection, Vector3 triangleNormal, Vector3 trianglePosition){

		//Set the rotation.
		LookRotationExtended(ref gameObjectInOut, alignWithVector, alignWithNormal, triangleDirection, triangleNormal);

		//Get the world space position of trianglePosition
		Vector3 trianglePositionWorld = gameObjectInOut.transform.TransformPoint(trianglePosition);

		//Get a vector from trianglePosition to alignWithPosition
		Vector3 translateVector = alignWithPosition - trianglePositionWorld;

		//Now transform the object so the triangle lines up correctly.
		gameObjectInOut.transform.Translate(translateVector, Space.World);
	}

	public static Vector4 QuaternionToVector4(Quaternion q){

			return new Vector4(q.x, q.y, q.z, q.w);
	}


	public static Quaternion Vector4ToQuaternion(Vector4 v){

			return new Quaternion(v.x, v.y, v.z, v.w);
	}

	public static Quaternion NormalizeQuaternion(float x, float y, float z, float w){

		float lengthD = 1.0f / (w*w + x*x + y*y + z*z);
		w *= lengthD;
		x *= lengthD;
		y *= lengthD;
		z *= lengthD;

		return new Quaternion(x, y, z, w);
	}

	//Changes the sign of the quaternion components. This is not the same as the inverse.
	public static Quaternion InverseSignQuaternion(Quaternion q){

		return new Quaternion(-q.x, -q.y, -q.z, -q.w);
	}

	//Returns true if the two input quaternions are close to each other. This can
	//be used to check whether or not one of two quaternions which are supposed to
	//be very similar but has its component signs reversed (q has the same rotation as
	//-q)
	public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2){

		float dot = Quaternion.Dot(q1, q2);

		if(dot < 0.0f){

			return false;					
		}

		else{

			return true;
		}
	}

	//Get an average (mean) from more then two quaternions (with two, slerp would be used).
	//Note: this only works if all the quaternions are relatively close together.
	//Usage: 
	//-Cumulative is an external Vector4 which holds all the added x y z and w components.
	//-newRotation is the next rotation to be added to the average pool
	//-firstRotation is the first quaternion of the array to be averaged
	//-addAmount holds the total amount of quaternions which are currently added
	//This function returns the current average quaternion
	public static Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount){

		float w = 0.0f;
		float x = 0.0f;
		float y = 0.0f;
		float z = 0.0f;

		//Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
		//q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
		if(!Math3d.AreQuaternionsClose(newRotation, firstRotation)){

			newRotation = Math3d.InverseSignQuaternion(newRotation);	
		}

		//Average the values
		float addDet = 1f/(float)addAmount;
		cumulative.w += newRotation.w;
		w = cumulative.w * addDet;
		cumulative.x += newRotation.x;
		x = cumulative.x * addDet;
		cumulative.y += newRotation.y;
		y = cumulative.y * addDet;
		cumulative.z += newRotation.z;
		z = cumulative.z * addDet;		

		//note: if speed is an issue, you can skip the normalization step
		return NormalizeQuaternion(x, y, z, w);
	}


	public static Quaternion VectorsToQuaternion(Vector3 direction, Vector3 normal){ 

		return Quaternion.LookRotation(direction, normal);
	}


	public static Vector3[] PointsToSpace(Vector3[] points, GameObject referenceObject, Space space){

		Vector3[] outPoints = new Vector3[points.Length];

		//convert to object space	
		if(space == Space.Self){

			for(int i = 0; i < points.Length; i++){				

				outPoints[i] = referenceObject.transform.InverseTransformPoint(points[i]);
			}	
		}

		//convert to world space
		if(space == Space.World){

			for(int i = 0; i < points.Length; i++){				

				outPoints[i] = referenceObject.transform.TransformPoint(points[i]);
			}	
		}

		return outPoints;
	}





	//This function finds out on which side of a line segment the point is located.
	//The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
	//the line segment, project it on the line using ProjectPointOnLine() first.
	//Returns 0 if point is on the line segment.
	//Returns 1 if point is outside of the line segment and located on the side of linePoint1.
	//Returns 2 if point is outside of the line segment and located on the side of linePoint2.
	public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point){

		Vector3 lineVec = linePoint2 - linePoint1;
		Vector3 pointVec = point - linePoint1;

		float dot = Vector3.Dot(pointVec, lineVec);

		//point is on side of linePoint2, compared to linePoint1
		if(dot > 0){

			//point is on the line segment
			if(pointVec.magnitude <= lineVec.magnitude){

				return 0;
			}

			//point is not on the line segment and it is on the side of linePoint2
			else{

				return 2;
			}
		}

		//Point is not on side of linePoint2, compared to linePoint1.
		//Point is not on the line segment and it is on the side of linePoint1.
		else{

			return 1;
		}
	}

	//Returns true if a line segment (made up of linePoint1 and linePoint2) is fully or partially in a rectangle
	//made up of RectA to RectD. The line segment is assumed to be on the same plane as the rectangle. If the line is 
	//not on the plane, use ProjectPointOnPlane() on linePoint1 and linePoint2 first.
	public static bool IsLineInRectangle(Vector3 linePoint1, Vector3 linePoint2, Vector3 rectA, Vector3 rectB, Vector3 rectC, Vector3 rectD){

		bool pointAInside = false;
		bool pointBInside = false;

		pointAInside = IsPointInRectangle(linePoint1, rectA, rectC, rectB, rectD);

		if(!pointAInside){

			pointBInside = IsPointInRectangle(linePoint2, rectA, rectC, rectB, rectD);
		}

		//none of the points are inside, so check if a line is crossing
		if(!pointAInside && !pointBInside){

			bool lineACrossing = AreLineSegmentsCrossing(linePoint1, linePoint2, rectA, rectB);
			bool lineBCrossing = AreLineSegmentsCrossing(linePoint1, linePoint2, rectB, rectC);
			bool lineCCrossing = AreLineSegmentsCrossing(linePoint1, linePoint2, rectC, rectD);
			bool lineDCrossing = AreLineSegmentsCrossing(linePoint1, linePoint2, rectD, rectA);

			if(lineACrossing || lineBCrossing || lineCCrossing || lineDCrossing){

				return true;
			}

			else{

				return false;
			}
		}

		else{

			return true;
		}
	}

	//Returns true if "point" is in a rectangle made up of RectA to RectD. The line point is assumed to be on the same 
	//plane as the rectangle. If the point is not on the plane, use ProjectPointOnPlane() first.
	public static bool IsPointInRectangle(Vector3 point, Vector3 rectA, Vector3 rectC, Vector3 rectB, Vector3 rectD){

		Vector3 vector;
		Vector3 linePoint;

		//get the center of the rectangle
		vector = rectC - rectA;
		float size = -(vector.magnitude / 2f);
		vector = AddVectorLength(vector, size);
		Vector3 middle = rectA + vector;

		Vector3 xVector = rectB - rectA;
		float width = xVector.magnitude / 2f;

		Vector3 yVector = rectD - rectA;
		float height = yVector.magnitude / 2f;

		linePoint = ProjectPointOnLine(middle, xVector.normalized, point);
		vector = linePoint - point;
		float yDistance = vector.magnitude;

		linePoint = ProjectPointOnLine(middle, yVector.normalized, point);
		vector = linePoint - point;
		float xDistance = vector.magnitude;

		if((xDistance <= width) && (yDistance <= height)){

			return true;
		}

		else{

			return false;
		}
	}

	//Returns true if line segment made up of pointA1 and pointA2 is crossing line segment made up of
	//pointB1 and pointB2. The two lines are assumed to be in the same plane.
	public static bool AreLineSegmentsCrossing(Vector3 pointA1, Vector3 pointA2, Vector3 pointB1, Vector3 pointB2){

		Vector3 closestPointA;
		Vector3 closestPointB;
		int sideA;
		int sideB;

		Vector3 lineVecA = pointA2 - pointA1;
		Vector3 lineVecB = pointB2 - pointB1;

		bool valid = ClosestPointsOnTwoLines(out closestPointA, out closestPointB, pointA1, lineVecA.normalized, pointB1, lineVecB.normalized); 

		//lines are not parallel
		if(valid){

			sideA = PointOnWhichSideOfLineSegment(pointA1, pointA2, closestPointA);
			sideB = PointOnWhichSideOfLineSegment(pointB1, pointB2, closestPointB);

			if((sideA == 0) && (sideB == 0)){

				return true;
			}

			else{

				return false;
			}
		}

		//lines are parallel
		else{

			return false;
		}
	}

	//This function returns a point which is a projection from a point to a line segment.
	//If the projected point lies outside of the line segment, the projected point will 
	//be clamped to the appropriate line edge.
	//If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
	public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point){

		Vector3 vector = linePoint2 - linePoint1;

		Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

		int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

		//The projected point is on the line segment
		if(side == 0){

			return projectedPoint;
		}

		if(side == 1){

			return linePoint1;
		}

		if(side == 2){

			return linePoint2;
		}

		//output is invalid
		return Vector3.zero;
	}

	//Returns the third vector to create a transform rotation. This is the normal of the two input vectors.
	//Output is based on the left hand rule where your thumb is vector A, your index finger is vector B, and your middle finger is the output vector.
	//Inputs need to be normalized first.
	public static Vector3 GetThirdVector(Vector3 A, Vector3 B){

		Vector3 C = Vector3.Cross(A, B);
		return C;
	}

	//Returns the pixel distance from the mouse pointer to a line.
	//Alternative for HandleUtility.DistanceToLine(). Works both in Editor mode and Play mode.
	//Do not call this function from OnGUI() as the mouse position will be wrong.
	public static float MouseDistanceToLine(Vector3 linePoint1, Vector3 linePoint2){

		Camera currentCamera;
		Vector3 mousePosition;

#if UNITY_EDITOR
		if(Camera.current != null){

			currentCamera = Camera.current;
		}

		else{

			currentCamera = Camera.main;
		}

		//convert format because y is flipped
		mousePosition = new Vector3(Event.current.mousePosition.x, currentCamera.pixelHeight - Event.current.mousePosition.y, 0f);

#else
		currentCamera = Camera.main;
		mousePosition = Input.mousePosition;
#endif
	
		Vector3 screenPos1 = currentCamera.WorldToScreenPoint(linePoint1);
		Vector3 screenPos2 = currentCamera.WorldToScreenPoint(linePoint2);
		Vector3 projectedPoint = ProjectPointOnLineSegment(screenPos1, screenPos2, mousePosition);

		//set z to zero
		projectedPoint = new Vector3(projectedPoint.x, projectedPoint.y, 0f);

		Vector3 vector = projectedPoint - mousePosition;
		return vector.magnitude;
	}


	//Returns the pixel distance from the mouse pointer to a camera facing circle.
	//Alternative for HandleUtility.DistanceToCircle(). Works both in Editor mode and Play mode.
	//Do not call this function from OnGUI() as the mouse position will be wrong.
	//If you want the distance to a point instead of a circle, set the radius to 0.
	public static float MouseDistanceToCircle(Vector3 point, float radius){

		Camera currentCamera;
		Vector3 mousePosition;

#if UNITY_EDITOR
		if(Camera.current != null){

			currentCamera = Camera.current;
		}

		else{

			currentCamera = Camera.main;
		}

		//convert format because y is flipped
		mousePosition = new Vector3(Event.current.mousePosition.x, currentCamera.pixelHeight - Event.current.mousePosition.y, 0f);
#else
		currentCamera = Camera.main;
		mousePosition = Input.mousePosition;
#endif

		Vector3 screenPos = currentCamera.WorldToScreenPoint(point);

		//set z to zero
		screenPos = new Vector3(screenPos.x, screenPos.y, 0f);

		Vector3 vector = screenPos - mousePosition;
		float fullDistance = vector.magnitude;
		float circleDistance = fullDistance - radius;

		return circleDistance;
	}
}
