using System;
using System.Collections.Generic;
using System.Threading;

namespace Console3DEnvironment
{
    class Program
    {
        static int screenWidth;
        static int screenHeight;
        static char[] screenBuffer;

        // Camera variables
        static Vector3 cameraPosition = new Vector3(0, 0, -5);
        static Vector3 cameraDirection = new Vector3(0, 0, 1);
        static float cameraYaw = 0.0f;
        static float cameraPitch = 0.0f;

        // Objects in the scene
        static List<Mesh> sceneObjects;

        static void Main(string[] args)
        {
            // Wait for user input to allow resizing the console
            Console.WriteLine("Press Enter to start the 3D Renderer. Resize the console window as desired.");
            Console.ReadLine();
            Console.Clear();

            // Display instructions
            DisplayInstructions();

            Console.CursorVisible = false;
            InitializeScreenBuffer();
            InitializeScene();

            // Main loop
            while (true)
            {
                HandleInput();
                RenderScene();
                Thread.Sleep(16); // Roughly 60 FPS
            }
        }

        static void DisplayInstructions()
        {
            Console.WriteLine("=== 3D Console Renderer Controls ===");
            Console.WriteLine("Movement:");
            Console.WriteLine("  W / S       : Move Forward / Backward");
            Console.WriteLine("  A / D       : Strafe Left / Right");
            Console.WriteLine("  Up/Down Arrow : Move Up / Down");
            Console.WriteLine("Camera Rotation:");
            Console.WriteLine("  Left/Right Arrow : Pan Left / Right (Yaw)");
            Console.WriteLine("  T / G       : Pan Up / Down (Pitch)");
            Console.WriteLine("Object Placement:");
            Console.WriteLine("  P           : Place a new Cube in Front of the Camera");
            Console.WriteLine("  O           : Remove the Last Placed Cube");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }

        static void InitializeScreenBuffer()
        {
            screenWidth = Console.WindowWidth;
            screenHeight = Console.WindowHeight;
            screenBuffer = new char[screenWidth * screenHeight];
        }

        static void InitializeScene()
        {
            // Initialize the list of scene objects
            sceneObjects = new List<Mesh>();

            // Create initial cubes in the scene
            sceneObjects.Add(Mesh.CreateCube(new Vector3(0, 0, 15)));
            sceneObjects.Add(Mesh.CreateCube(new Vector3(5, 0, 25)));
            sceneObjects.Add(Mesh.CreateCube(new Vector3(-5, 0, 25)));
            sceneObjects.Add(Mesh.CreateCube(new Vector3(0, 5, 20)));
            sceneObjects.Add(Mesh.CreateCube(new Vector3(0, -5, 20)));
        }

        static void HandleInput()
        {
            while (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(true);
                var key = keyInfo.Key;

                // Movement and rotation speed
                float moveSpeed = 0.5f;
                float turnSpeed = 1.0f;

                switch (key)
                {
                    // Move forward and backward
                    case ConsoleKey.W:
                        cameraPosition += cameraDirection * moveSpeed;
                        break;
                    case ConsoleKey.S:
                        cameraPosition -= cameraDirection * moveSpeed;
                        break;
                    // Strafe left and right
                    case ConsoleKey.A:
                        cameraPosition -= Vector3.Cross(new Vector3(0, 1, 0), cameraDirection) * moveSpeed;
                        break;
                    case ConsoleKey.D:
                        cameraPosition += Vector3.Cross(new Vector3(0, 1, 0), cameraDirection) * moveSpeed;
                        break;
                    // Move up and down
                    case ConsoleKey.UpArrow:
                        cameraPosition.Y += moveSpeed;
                        break;
                    case ConsoleKey.DownArrow:
                        cameraPosition.Y -= moveSpeed;
                        break;
                    // Pan left and right
                    case ConsoleKey.LeftArrow:
                        cameraYaw -= turnSpeed;
                        UpdateCameraDirection();
                        break;
                    case ConsoleKey.RightArrow:
                        cameraYaw += turnSpeed;
                        UpdateCameraDirection();
                        break;
                    // Pan up and down
                    case ConsoleKey.T:
                        cameraPitch += turnSpeed;
                        UpdateCameraDirection();
                        break;
                    case ConsoleKey.G:
                        cameraPitch -= turnSpeed;
                        UpdateCameraDirection();
                        break;
                    // Object placement
                    case ConsoleKey.P:
                        PlaceNewCube();
                        break;
                    case ConsoleKey.O:
                        RemoveLastCube();
                        break;
                    default:
                        // Check for window resize
                        if (Console.WindowWidth != screenWidth || Console.WindowHeight != screenHeight)
                        {
                            InitializeScreenBuffer();
                        }
                        break;
                }
            }
        }

        static void UpdateCameraDirection()
        {
            float yawRad = DegreesToRadians(cameraYaw);
            float pitchRad = DegreesToRadians(cameraPitch);

            cameraDirection.X = (float)(Math.Cos(pitchRad) * Math.Sin(yawRad));
            cameraDirection.Y = (float)(Math.Sin(pitchRad));
            cameraDirection.Z = (float)(Math.Cos(pitchRad) * Math.Cos(yawRad));
            cameraDirection = Vector3.Normalize(cameraDirection);
        }

        static float DegreesToRadians(float degrees)
        {
            return (float)(Math.PI / 180 * degrees);
        }

        static void PlaceNewCube()
        {
            // Define the distance in front of the camera to place the new cube
            float distance = 5.0f;
            Vector3 newPosition = cameraPosition + cameraDirection * distance;

            // Optional: Slightly offset vertically to avoid overlapping with the camera
            newPosition.Y += 0;

            // Add the new cube to the scene
            sceneObjects.Add(Mesh.CreateCube(newPosition));
        }

        static void RemoveLastCube()
        {
            if (sceneObjects.Count > 0)
            {
                sceneObjects.RemoveAt(sceneObjects.Count - 1);
            }
        }

        static void RenderScene()
        {
            // Clear the screen buffer
            for (int i = 0; i < screenBuffer.Length; i++)
            {
                screenBuffer[i] = ' ';
            }

            // Projection parameters
            float fov = 90.0f;
            float aspectRatio = (float)screenWidth / screenHeight; // Corrected aspect ratio
            float near = 0.1f;
            float far = 1000.0f;
            float fovRad = (float)(1.0f / Math.Tan(DegreesToRadians(fov) * 0.5f));

            // Projection matrix
            Matrix4x4 projectionMatrix = new Matrix4x4(
                aspectRatio * fovRad, 0, 0, 0,
                0, fovRad, 0, 0,
                0, 0, far / (far - near), 1,
                0, 0, (-far * near) / (far - near), 0
            );

            foreach (var mesh in sceneObjects)
            {
                foreach (var triangle in mesh.Triangles)
                {
                    // Back-Face Culling: Calculate the normal of the triangle
                    Vector3 line1 = triangle.Point1 - triangle.Point0;
                    Vector3 line2 = triangle.Point2 - triangle.Point0;
                    Vector3 normal = Vector3.Cross(line1, line2);
                    normal = Vector3.Normalize(normal);

                    // Vector from camera to triangle
                    Vector3 cameraToTriangle = triangle.Point0 - cameraPosition;

                    // If the dot product is less than zero, the triangle is facing the camera
                    if (Vector3.Dot(normal, cameraToTriangle) < 0)
                    {
                        Triangle transformedTriangle = new Triangle();

                        // World transformation: Translate relative to camera
                        transformedTriangle.Point0 = triangle.Point0 - cameraPosition;
                        transformedTriangle.Point1 = triangle.Point1 - cameraPosition;
                        transformedTriangle.Point2 = triangle.Point2 - cameraPosition;

                        // Camera rotation: Apply rotation matrix
                        Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(
                            DegreesToRadians(cameraYaw),
                            DegreesToRadians(cameraPitch),
                            0);
                        transformedTriangle.Point0 = Vector3.Transform(transformedTriangle.Point0, rotationMatrix);
                        transformedTriangle.Point1 = Vector3.Transform(transformedTriangle.Point1, rotationMatrix);
                        transformedTriangle.Point2 = Vector3.Transform(transformedTriangle.Point2, rotationMatrix);

                        // Projection
                        Vector3[] projectedPoints3D = new Vector3[3];
                        for (int i = 0; i < 3; i++)
                        {
                            Vector3 point;
                            switch (i)
                            {
                                case 0: point = transformedTriangle.Point0; break;
                                case 1: point = transformedTriangle.Point1; break;
                                case 2: point = transformedTriangle.Point2; break;
                                default: point = new Vector3(0, 0, 0); break;
                            }

                            Vector4 v = new Vector4(point.X, point.Y, point.Z, 1);
                            Vector4 projectedPoint4D = projectionMatrix.MultiplyVector(v);
                            Vector3 projectedPoint = new Vector3(projectedPoint4D.X, projectedPoint4D.Y, projectedPoint4D.Z);

                            if (projectedPoint.Z == 0)
                                projectedPoint.Z = 0.0001f;

                            projectedPoint.X /= projectedPoint.Z;
                            projectedPoint.Y /= projectedPoint.Z;

                            // Scale into view
                            projectedPoint.X += 1.0f;
                            projectedPoint.Y += 1.0f;
                            projectedPoint.X *= 0.5f * screenWidth;
                            projectedPoint.Y *= 0.5f * screenHeight;

                            projectedPoints3D[i] = projectedPoint;
                        }

                        // Create a new triangle with projected points
                        Triangle projectedTriangle = new Triangle(
                            projectedPoints3D[0],
                            projectedPoints3D[1],
                            projectedPoints3D[2]
                        );

                        // Rasterize triangle
                        DrawTriangle(projectedTriangle);
                    }
                }
            }

            // Draw the buffer
            try
            {
                Console.SetCursorPosition(0, 0);
                for (int y = 0; y < screenHeight; y++)
                {
                    if (y * screenWidth + screenWidth > screenBuffer.Length)
                        break; // Prevent out-of-range errors

                    string line = new string(screenBuffer, y * screenWidth, screenWidth);
                    Console.Write(line);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Handle cases where console size changes during rendering
                // You can choose to ignore or reset the buffer
            }
        }

        static void DrawTriangle(Triangle triangle)
        {
            // Convert points to integers
            int x1 = Clamp((int)triangle.Point0.X, 0, screenWidth - 1);
            int y1 = Clamp((int)triangle.Point0.Y, 0, screenHeight - 1);
            int x2 = Clamp((int)triangle.Point1.X, 0, screenWidth - 1);
            int y2 = Clamp((int)triangle.Point1.Y, 0, screenHeight - 1);
            int x3 = Clamp((int)triangle.Point2.X, 0, screenWidth - 1);
            int y3 = Clamp((int)triangle.Point2.Y, 0, screenHeight - 1);

            DrawLine(x1, y1, x2, y2);
            DrawLine(x2, y2, x3, y3);
            DrawLine(x3, y3, x1, y1);
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        static void DrawLine(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            while (true)
            {
                if (x0 >= 0 && x0 < screenWidth && y0 >= 0 && y0 < screenHeight)
                {
                    screenBuffer[y0 * screenWidth + x0] = '#';
                }

                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }

    // Custom Vector3 struct
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // Addition
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        // Subtraction
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        // Scalar multiplication
        public static Vector3 operator *(Vector3 a, float scalar)
        {
            return new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }

        // Cross product
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        // Dot product
        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        // Normalize
        public static Vector3 Normalize(Vector3 v)
        {
            float length = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
            if (length == 0) return new Vector3(0, 0, 0);
            return new Vector3(v.X / length, v.Y / length, v.Z / length);
        }

        // Transform by Matrix4x4
        public static Vector3 Transform(Vector3 vector, Matrix4x4 matrix)
        {
            float x = vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41;
            float y = vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42;
            float z = vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43;
            float w = vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44;

            if (w != 0 && w != 1)
            {
                x /= w;
                y /= w;
                z /= w;
            }

            return new Vector3(x, y, z);
        }
    }

    // Custom Vector4 struct
    public struct Vector4
    {
        public float X, Y, Z, W;

        public Vector4(float x, float y, float z, float w = 1)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    // Custom Matrix4x4 struct
    public struct Matrix4x4
    {
        public float M11, M12, M13, M14;
        public float M21, M22, M23, M24;
        public float M31, M32, M33, M34;
        public float M41, M42, M43, M44;

        // Constructor
        public Matrix4x4(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44
        )
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        // Create rotation matrix from yaw, pitch, and roll
        public static Matrix4x4 CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);
            float cosPitch = (float)Math.Cos(pitch);
            float sinPitch = (float)Math.Sin(pitch);
            float cosRoll = (float)Math.Cos(roll);
            float sinRoll = (float)Math.Sin(roll);

            Matrix4x4 matrix = new Matrix4x4();

            matrix.M11 = cosYaw * cosRoll + sinYaw * sinPitch * sinRoll;
            matrix.M12 = sinRoll * cosPitch;
            matrix.M13 = -sinYaw * cosRoll + cosYaw * sinPitch * sinRoll;
            matrix.M14 = 0;

            matrix.M21 = -cosYaw * sinRoll + sinYaw * sinPitch * cosRoll;
            matrix.M22 = cosRoll * cosPitch;
            matrix.M23 = sinRoll * sinYaw + cosYaw * sinPitch * cosRoll;
            matrix.M24 = 0;

            matrix.M31 = sinYaw * cosPitch;
            matrix.M32 = -sinPitch;
            matrix.M33 = cosYaw * cosPitch;
            matrix.M34 = 0;

            matrix.M41 = 0;
            matrix.M42 = 0;
            matrix.M43 = 0;
            matrix.M44 = 1;

            return matrix;
        }

        // Multiply Matrix4x4 by Vector4 using a method
        public Vector4 MultiplyVector(Vector4 v)
        {
            return new Vector4(
                v.X * M11 + v.Y * M21 + v.Z * M31 + v.W * M41,
                v.X * M12 + v.Y * M22 + v.Z * M32 + v.W * M42,
                v.X * M13 + v.Y * M23 + v.Z * M33 + v.W * M43,
                v.X * M14 + v.Y * M24 + v.Z * M34 + v.W * M44
            );
        }
    }

    // Mesh class representing an object in the scene
    class Mesh
    {
        public Triangle[] Triangles;

        public static Mesh CreateCube(Vector3 position)
        {
            // Define the 8 vertices of a cube
            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(-1, -1, -1) + position;
            vertices[1] = new Vector3(1, -1, -1) + position;
            vertices[2] = new Vector3(1, 1, -1) + position;
            vertices[3] = new Vector3(-1, 1, -1) + position;
            vertices[4] = new Vector3(-1, -1, 1) + position;
            vertices[5] = new Vector3(1, -1, 1) + position;
            vertices[6] = new Vector3(1, 1, 1) + position;
            vertices[7] = new Vector3(-1, 1, 1) + position;

            // Define the 12 triangles composing the cube
            Triangle[] triangles = new Triangle[12];

            // South face
            triangles[0] = new Triangle(vertices[0], vertices[1], vertices[2]);
            triangles[1] = new Triangle(vertices[0], vertices[2], vertices[3]);

            // East face
            triangles[2] = new Triangle(vertices[1], vertices[5], vertices[6]);
            triangles[3] = new Triangle(vertices[1], vertices[6], vertices[2]);

            // North face
            triangles[4] = new Triangle(vertices[5], vertices[4], vertices[7]);
            triangles[5] = new Triangle(vertices[5], vertices[7], vertices[6]);

            // West face
            triangles[6] = new Triangle(vertices[4], vertices[0], vertices[3]);
            triangles[7] = new Triangle(vertices[4], vertices[3], vertices[7]);

            // Top face
            triangles[8] = new Triangle(vertices[3], vertices[2], vertices[6]);
            triangles[9] = new Triangle(vertices[3], vertices[6], vertices[7]);

            // Bottom face
            triangles[10] = new Triangle(vertices[4], vertices[5], vertices[1]);
            triangles[11] = new Triangle(vertices[4], vertices[1], vertices[0]);

            return new Mesh { Triangles = triangles };
        }
    }

    // Triangle struct representing a triangle with 3 vertices
    struct Triangle
    {
        public Vector3 Point0;
        public Vector3 Point1;
        public Vector3 Point2;

        public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Point0 = p0;
            Point1 = p1;
            Point2 = p2;
        }
    }
}
