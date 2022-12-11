using System;
using System.Collections.Generic;
using UnityEngine;


public class CCMeshData
{
    public List<Vector3> points; // Original mesh points
    public List<Vector4> faces; // Original mesh quad faces
    public List<Vector4> edges; // Original mesh edges
    public List<Vector3> facePoints; // Face points, as described in the Catmull-Clark algorithm
    public List<Vector3> edgePoints; // Edge points, as described in the Catmull-Clark algorithm
    public List<Vector3> newPoints; // New locations of the original mesh points, according to Catmull-Clark
}


public static class CatmullClark
{
    // Returns a QuadMeshData representing the input mesh after one iteration of Catmull-Clark subdivision.
    public static QuadMeshData Subdivide(QuadMeshData quadMeshData)
    {
        // Create and initialize a CCMeshData corresponding to the given QuadMeshData
        CCMeshData meshData = new CCMeshData();
        meshData.points = quadMeshData.vertices;
        meshData.faces = quadMeshData.quads;
        meshData.edges = GetEdges(meshData);
        meshData.facePoints = GetFacePoints(meshData);
        meshData.edgePoints = GetEdgePoints(meshData);
        meshData.newPoints = GetNewPoints(meshData);

        // Combine facePoints, edgePoints and newPoints into a subdivided QuadMeshData

        // Your implementation here...

        return new QuadMeshData();
    }

    // Returns a list of all edges in the mesh defined by given points and faces.
    // Each edge is represented by Vector4(p1, p2, f1, f2)
    // p1, p2 are the edge vertices
    // f1, f2 are faces incident to the edge. If the edge belongs to one face only, f2 is -1
    public static List<Vector4> GetEdges(CCMeshData mesh)
    {
        Dictionary<Vector2, Vector4> dictionary = new Dictionary<Vector2, Vector4>();
        for (int j=0; j<mesh.faces.Count; j++)
        {
            for (int i = 0; i < 4; i++)
            {
                float p1 = mesh.faces[j][i];
                float p2 = mesh.faces[j][(i + 1) % 4];
                Vector2 pointsBack = new Vector2(p2, p1);
                if (dictionary.ContainsKey(pointsBack))
                {
                    Vector4 edge = dictionary[pointsBack];
                    edge.w = j;
                    dictionary[pointsBack] = edge;
                }
                else
                {
                    Vector4 edge = new Vector4(p1, p2, j, -1);
                    dictionary[pointsBack] = edge;
                }
            }
        }
        List<Vector4> edges = new List<Vector4>();
        foreach (var edge in dictionary.Values)
            edges.Add(edge);
        return edges;
    }

    // Returns a list of "face points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetFacePoints(CCMeshData mesh)
    {
        List<Vector3> facePoints = new List<Vector3>();
        for (int i = 0; i < mesh.faces.Count; i++)
        {
            Vector3 p1 = mesh.points[(int)mesh.faces[i].x];
            Vector3 p2 = mesh.points[(int)mesh.faces[i].y];
            Vector3 p3 = mesh.points[(int)mesh.faces[i].z];
            Vector3 p4 = mesh.points[(int)mesh.faces[i].w];
            facePoints.Add((p1 + p2 + p3 + p4)/4);
        }
        return facePoints;
    }

    // Returns a list of "edge points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetEdgePoints(CCMeshData mesh)
    {
        List<Vector3> edgesPoints = new List<Vector3>();
        for (int i = 0; i < mesh.edges.Count; i++)
        {
            Vector3 p1 = mesh.points[(int)mesh.edges[i].x];
            Vector3 p2 = mesh.points[(int)mesh.edges[i].y];
            Vector3 f1 = mesh.facePoints[(int)mesh.edges[i].z];
            Vector3 f2 = mesh.facePoints[(int)mesh.edges[i].w];
            edgesPoints.Add((p1 + p2 + f1 + f2)/4);
        }
        return edgesPoints;
    }

    // Returns a list of new locations of the original points for the given CCMeshData, as described in the CC algorithm 
    public static List<Vector3> GetNewPoints(CCMeshData mesh)
    {
        return null;
    }
}
