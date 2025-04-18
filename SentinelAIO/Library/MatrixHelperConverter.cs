using System.Numerics;

namespace SentinelAIO.Library;

public class MatrixHelperConverter
{
    private string coordinateSystem = "Maya";
    private string coordinateSystem2 = "LoL";
    private string rotationOrder = "XYZ";


    public void SetRotationOrder(string order, string system)
    {
        rotationOrder = order;
        coordinateSystem = system;
    }

    public float RadiansToDegrees(float radians)
    {
        return radians * (180.0f / (float)Math.PI);
    }

    public float DegreesToRadians(float degrees)
    {
        return degrees * ((float)Math.PI / 180.0f);
    }

    public float Clamp(float value, float min, float max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    public void SetRotationOrder(string order)
    {
        rotationOrder = order;
    }

    public (Quaternion, Matrix4x4, Vector3, Vector3) EulerToQuaternionAndMatrix(Vector3 euler, Vector3 translate,
        Vector3 scale)
    {
        Quaternion quaternion;
        Matrix4x4 matrix;


        quaternion = Quaternion.CreateFromYawPitchRoll(euler.Y, euler.X, euler.Z);

        // Create a scaling matrix from the scale vector.
        var matrixScale = Matrix4x4.CreateScale(scale);

        // Create a translation matrix from the translation vector.
        var matrixTranslation = Matrix4x4.CreateTranslation(translate);

        // Combine all matrices. Multiply the scale, then rotation, then translation.
        // Order of multiplication matters in linear algebra.
        matrix = matrixScale * Matrix4x4.CreateFromQuaternion(quaternion) * matrixTranslation;


        return (quaternion, matrix, translate, scale);
    }

    public (Vector3, Matrix4x4) QuaternionToEulerAndMatrix(Quaternion quaternion)
    {
        var euler = new Vector3
        {
            X = (float)Math.Asin(2.0 * (quaternion.W * quaternion.X - quaternion.Y * quaternion.Z)),
            Y = (float)Math.Atan2(2.0 * (quaternion.W * quaternion.Y + quaternion.Z * quaternion.X),
                1.0 - 2.0 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y)),
            Z = (float)Math.Atan2(2.0 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y),
                1.0 - 2.0 * (quaternion.Z * quaternion.Z + quaternion.X * quaternion.X))
        };
        var matrix = Matrix4x4.CreateFromQuaternion(quaternion);
        return (euler, matrix);
    }

    public (Quaternion, Vector3, Vector3, Vector3) MatrixToQuaternionAndEuler(Matrix4x4 matrix)
    {
        var quaternion = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(matrix));
        var euler = QuaternionToEulerAndMatrix(quaternion).Item1;
        var translate = new Vector3(matrix.M41, matrix.M42, matrix.M43);
        var scale = new Vector3(
            (float)Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12 + matrix.M13 * matrix.M13),
            (float)Math.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22 + matrix.M23 * matrix.M23),
            (float)Math.Sqrt(matrix.M31 * matrix.M31 + matrix.M32 * matrix.M32 + matrix.M33 * matrix.M33)
        );


        //League inverts X for Translate. Mobs and Particles have an inverted X and Z Scale but I am not sure (Because of Maya Plugin)
        euler = new Vector3(euler.X, euler.Y, euler.Z);
        translate = new Vector3(-translate.X, translate.Y, translate.Z);
        scale = new Vector3(-scale.X, scale.Y, -scale.Z);


        return (quaternion, euler, translate, scale);
    }
}